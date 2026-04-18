using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }


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

            // 1. Lekérjük a szabályokat az aktuális termékre
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
            return await _context.Products
                .Include(p => p.Category)   // Hogy lássuk a kategória nevét
                .Include(p => p.StockItems)  // Hogy lássuk a készletet
                .ToListAsync();

        }

        public async Task ProcessStockReceiptAsync(int productId, int warehouseId, decimal quantity, decimal unitPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Termék nem található.");

            var stock = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (stock == null)
            {
                stock = new StockItem { ProductId = productId, Quantity = 0, WarehouseId = warehouseId };
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

            stock.Quantity = newQuantity;

            await _context.SaveChangesAsync();
        }

        public async Task TransferStockAsync(int productId, int fromWarehouseId, int toWarehouseId, decimal quantity)
        {
            if (fromWarehouseId == toWarehouseId) throw new Exception("A forrás és cél raktár nem lehet ugyanaz.");

            var sourceStock = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == fromWarehouseId);

            var targetStock = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == toWarehouseId);

            if (sourceStock == null || sourceStock.Quantity < quantity)
                throw new Exception("Nincs elegendő készlet a kiinduló raktárban.");

            if (targetStock == null)
            {
                targetStock = new StockItem { ProductId = productId, Quantity = 0, WarehouseId = toWarehouseId };
                _context.StockItems.Add(targetStock);
            }

            sourceStock.Quantity -= quantity;
            targetStock.Quantity += quantity;
            await _context.SaveChangesAsync();
        }

        public async Task GetProductsByCategory(int categoryId)
        {
            await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
        }

        public async Task ProcessStockIssueAsync(int productId, int warehouseId, decimal quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            var stock = await _context.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            if (product == null || stock == null)
                throw new Exception("Termék vagy készlet nem található.");
            if (stock.Quantity < quantity)
                throw new Exception($"Nincs elegendő készlet a termékhez. Elérhető:{stock.Quantity}, kiadni kívánt: {quantity}");

            stock.Quantity -= quantity;

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<object>> GetStockByWarehouseAsync(int warehouseId)
        {
            return await _context.StockItems
                .Include(s => s.Product)
                .Where(s => s.WarehouseId == warehouseId) 
                .Select(s => new {
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    SKU = s.Product.SKU,
                    Quantity = s.Quantity,
                    Unit = s.Product.Unit.ToString(), // db, kg, l...
                    AveragePrice = s.Product.CurrentAveragePrice,
                    SubTotalValue = s.Quantity * s.Product.CurrentAveragePrice
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetInventorySummaryAsync()
        {
            var allItems = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .ToListAsync();

            return allItems
                .GroupBy(s => s.ProductId)
                .Select(group => new {
                    ProductId = group.Key,
                    ProductName = group.First().Product.Name,
                    SKU = group.First().Product.SKU,

                    TotalQuantity = group.Sum(s => s.Quantity),
                    AveragePrice = group.First().Product.CurrentAveragePrice,

                    TotalInventoryValue = group.Sum(s => s.Quantity) * group.First().Product.CurrentAveragePrice,

                    WarehouseDistribution = group.Select(s => new {
                        WarehouseName = s.Warehouse?.Name ?? "Nincs kijelölve",
                        WarehouseId = s.WarehouseId,
                        Quantity = s.Quantity
                    })
                });
        }
        public async Task<IEnumerable<object>> GetProductLocationsAsync(int productId, int quantity)
        {
            return await _context.StockItems
                .Where(s => s.ProductId == productId && s.Quantity >= quantity) // Csak ott, ahol tényleg van készlet
                .Include(s => s.Warehouse)
                .Select(s => new {
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "Nincs név",
                    CurrentStock = s.Quantity,
                    Unit = s.Product.Unit.ToString()
                })
                .ToListAsync();
        }
    }
}