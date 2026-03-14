using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection: itt kérjük el a DbContextet
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Mostantól ide kell írnod a metódusok belsejét ---

        public async Task CreateProductAsync(Product product)
        {
            // 1. Adat-tisztítás
            product.SKU = product.SKU.Trim().ToUpper();
            product.Name = product.Name.Trim();

            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null)
                throw new ArgumentException("A megadott kategória nem létezik.");

            product.Category = category;

            // 2. SKU egyediség ellenőrzése
            if (await _context.Products.AnyAsync(p => p.SKU == product.SKU))
                throw new InvalidOperationException("Már létezik termék ezzel az SKU-val.");

            // 3. Kategória ellenőrzés (Hogy ne legyen árva rekord)
            if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
                throw new ArgumentException("A megadott kategória nem létezik.");

            // 1. Kérjük le a szabályokat az aktuális termékre
            var rules = RuleProvider.GetRulesForProduct(product);

            // 2. Futtassuk le őket
            foreach (var rule in rules)
            {
                rule.ValidateProduct(product);
            }

            // 5. Szerver oldali alapértelmezett értékek
            product.DateCreated = DateTime.UtcNow;

            // 6. Mentés
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product != null)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("A megadott termék nem található.");
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("A megadott termék nem található.");
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }


        public async Task<IEnumerable<Product?>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task ProcessStockReceiptAsync(int productId, decimal quantity, decimal unitPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Termék nem található.");

            var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId);

            if (stock == null)
            {
                stock = new StockItem { ProductId = productId, Quantity = 0, WarehouseId = null };
                _context.StockItems.Add(stock);
            }


            decimal oldQuantity = stock.Quantity;
            decimal newQuantity = oldQuantity + quantity;

            if (newQuantity > 0)
            {
                // Súlyozott átlagár 
                product.CurrentAveragePrice = ((product.CurrentAveragePrice * oldQuantity) + (unitPrice * quantity)) / newQuantity;
            }
            else
            {
                product.CurrentAveragePrice = unitPrice;
            }

            // Csak a számítás UTÁN frissítjük a készletet az adatbázis objektumban
            stock.Quantity = newQuantity;

            await _context.SaveChangesAsync();
        }

        public async Task GetProductsByCategory(int categoryId)
        {
            await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
        }

        public async Task ProcessStockIssueAsync(int productId, decimal quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId);

            if (product == null || stock == null)
                throw new Exception("Termék vagy készlet nem található.");
            if (stock.Quantity < quantity)
                throw new Exception($"Nincs elegendő készlet a termékhez. Elérhető:{stock.Quantity}, kiadni kívánt: {quantity}");

            stock.Quantity -= quantity;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<object>> GetInventorySummaryAsync()
        {
            return await _context.StockItems
                .Include(s => s.Product)
                .Select(s => new {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    SKU = s.Product.SKU,
                    CurrentStock = s.Quantity,
                    AveragePrice = s.Product.CurrentAveragePrice,
                    TotalValue = s.Quantity * s.Product.CurrentAveragePrice
                })
                .ToListAsync();
        }
    }
}