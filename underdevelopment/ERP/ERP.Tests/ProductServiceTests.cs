using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ERP.Data;
using ERP.Models;
using ERP.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ERP.Tests.Services
{
    public class ProductServiceTests
    {
        // Segédfüggvény egy tiszta, memóriában futó adatbázis létrehozásához minden teszthez
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Minden teszt új adatbázist kap
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task ProcessStockReceipt_UpdatesAveragePrice_And_MinMaxPrices_Correctly()
        {
            // Arrange (Előkészítés)
            using var context = GetInMemoryDbContext();

            // Dummy AuditService
            var service = new ProductService(context, null);

            var product = new Product { Id = 1, Name = "Teszt Alma", SKU = "ALMA-1", DateCreated = DateTime.Now };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act (Cselekvés) - Két különböző árú bevételezés
            // 1. lépés: 10 db bevételezése 100 Ft-ért
            await service.ProcessStockReceiptAsync(1, 1, 10, 100);

            // 2. lépés: 10 db bevételezése 200 Ft-ért
            await service.ProcessStockReceiptAsync(1, 1, 10, 200);

            // Assert (Ellenőrzés)
            var updatedProduct = await context.Products.FindAsync(1);

            // Az átlagárnak pontosan 150 Ft-nak kell lennie ( (10*100 + 10*200) / 20 )
            Assert.Equal(150, updatedProduct.CurrentAveragePrice);

            // A legkisebb ár 100, a legnagyobb 200 kell, hogy legyen
            Assert.Equal(100, updatedProduct.MinPurchasePrice);
            Assert.Equal(200, updatedProduct.MaxPurchasePrice);
        }

        [Fact]
        public async Task ProcessStockReceipt_PerishableProduct_WithoutDate_ThrowsException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new ProductService(context, null);

            var perishableProduct = new Product
            {
                Id = 2,
                Name = "Tej",
                SKU = "TEJ-1",
                IsPerishable = true, // ROMLANDÓ!
                DateCreated = DateTime.Now
            };
            context.Products.Add(perishableProduct);
            await context.SaveChangesAsync();

            // Act & Assert
            // Megpróbáljuk bevételezni dátum nélkül (null), és elvárjuk, hogy elszálljon a megfelelő hibával
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ProcessStockReceiptAsync(2, 1, 10, 300, expirationDate: null)
            );

            Assert.Contains("kötelező megadni a lejárati dátumot", exception.Message);
        }

        [Fact]
        public async Task ProcessStockIssue_AppliesFefoLogic_TakesFromEarliestExpiryFirst()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // 2. Létrehozzuk az AuditService-t, átadva mindkét paramétert
            var auditService = new AuditService(context, mockHttpContextAccessor.Object);

            // 3. Most már átadhatjuk a ProductService-nek
            var service = new ProductService(context, auditService);

            var product = new Product { Id = 3, Name = "Kenyér", SKU = "KENYER-1", DateCreated = DateTime.Now };
            context.Products.Add(product);

            // Csinálunk két adagot: egyet ami holnap jár le, egyet ami jövő héten
            var batchExpiringTomorrow = new StockItem
            {
                ProductId = 3,
                WarehouseId = 1,
                Quantity = 5,
                ExpirationDate = DateTime.Now.AddDays(1)
            };
            var batchExpiringNextWeek = new StockItem
            {
                ProductId = 3,
                WarehouseId = 1,
                Quantity = 10,
                ExpirationDate = DateTime.Now.AddDays(7)
            };

            context.StockItems.AddRange(batchExpiringTomorrow, batchExpiringNextWeek);
            await context.SaveChangesAsync();

            // Act
            // Kiadunk 7 darabot. A FEFO miatt először a holnap lejáró 5 db-ot kell megennie, 
            // aztán a maradék 2 db-ot a jövő heti adagból.
            await service.ProcessStockIssueAsync(3, 1, 7);

            // Assert
            var stocks = context.StockItems.Where(s => s.ProductId == 3).OrderBy(s => s.ExpirationDate).ToList();

            Assert.Equal(0, stocks[0].Quantity); // A holnap lejáró készletnek 0-nak kell lennie
            Assert.Equal(8, stocks[1].Quantity); // A jövő héten lejáró készletből 8 db-nak kell maradnia
        }
    }
}