using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        // Dependency Injection
        public ProductService(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;

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
                await _audit.LogAsync("PRODUCT_DELETE", $"Törölt termék: {product}");
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("A megadott termék nem található.");
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.StockItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<IEnumerable<Product?>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)   // Hogy lássuk a kategória nevét
                .Include(p => p.StockItems)  // Hogy lássuk a készletet
                .ToListAsync();

        }

        public async Task ProcessStockReceiptAsync(int productId, int warehouseId, decimal quantity, decimal unitPrice, DateTime? expirationDate = null)
        {
            var product = await _context.Products
                .Include(p => p.StockItems)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) throw new Exception("Termék nem található.");

            // 1. DÁTUM ELLENŐRZÉS (Ha romlandó, de nincs dátum -> HIBA)
            if (product.IsPerishable && !expirationDate.HasValue)
            {
                throw new InvalidOperationException("Ennél a romlandó terméknél kötelező megadni a lejárati dátumot a bevételezéskor!");
            }

            if (product.IsPerishable && expirationDate.HasValue && expirationDate.Value.Date < DateTime.Now.Date)
            {
              throw new InvalidOperationException($"Hiba: A termék szavatossága már lejárt ({expirationDate.Value.ToShortDateString()})! Nem vehető át.");
            }

            // 2. MIN / MAX ÁR FRISSÍTÉSE
            if (product.MinPurchasePrice == 0 || unitPrice < product.MinPurchasePrice)
                product.MinPurchasePrice = unitPrice;

            if (unitPrice > product.MaxPurchasePrice)
                product.MaxPurchasePrice = unitPrice;

            // 3. ÁTLAGÁR SZÁMÍTÁS (WAC)
            decimal totalGlobalQuantityBefore = product.StockItems.Sum(s => s.Quantity);
            decimal totalGlobalQuantityAfter = totalGlobalQuantityBefore + quantity;

            if (totalGlobalQuantityAfter > 0)
            {
                product.CurrentAveragePrice =
                    ((product.CurrentAveragePrice * totalGlobalQuantityBefore) + (unitPrice * quantity))
                    / totalGlobalQuantityAfter;
            }
            else
            {
                product.CurrentAveragePrice = unitPrice;
            }

            product.PurchasePrice = unitPrice; // Utolsó beszerzési ár

            // 4. KÉSZLET HOZZÁADÁSA (Keresünk azonos lejáratú adagot, ha nincs, újat csinálunk)
            var targetStock = product.StockItems.FirstOrDefault(s => s.WarehouseId == warehouseId && s.ExpirationDate == expirationDate);

            if (targetStock == null)
            {
                targetStock = new StockItem
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Quantity = 0,
                    ExpirationDate = expirationDate // Itt kapja meg a konkrét adag a dátumot!
                };
                _context.StockItems.Add(targetStock);
            }

            targetStock.Quantity += quantity;

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

            var fromWarehouseName = await _context.Warehouses
                .Where(w => w.Id == fromWarehouseId)
                .Select(w => w.Name)
                .FirstOrDefaultAsync();

            var toWarehouseName = await _context.Warehouses
                 .Where(w => w.Id == toWarehouseId)
                 .Select(w => w.Name)
                 .FirstOrDefaultAsync(); 



            await _audit.LogAsync("STOCK_MOVED", $"Termék Id: {productId}, Mennyiség: {quantity}, Inenn: {fromWarehouseName}, Ide: {toWarehouseName}");


            await _context.SaveChangesAsync();
        }

        public async Task GetProductsByCategory(int categoryId)
        {
            await _context.Products.Where(p => p.CategoryId == categoryId).ToListAsync();
        }

        public async Task ProcessStockIssueAsync(int productId, int warehouseId, decimal quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Termék nem található.");

            // FEFO SORREND: Lekérjük a készlet-adagokat. 
            // Aminek van lejárata, előre kerül. Dátum szerint növekvő sorrend (leghamarabb lejáró az első).
            var stocks = await _context.StockItems
                .Where(s => s.ProductId == productId && s.WarehouseId == warehouseId && s.Quantity > 0)
                .OrderBy(s => s.ExpirationDate.HasValue ? 0 : 1)
                .ThenBy(s => s.ExpirationDate)
                .ToListAsync();

            decimal totalAvailable = stocks.Sum(s => s.Quantity);

            if (totalAvailable < quantity)
                throw new Exception($"Nincs elegendő készlet a termékhez. Elérhető: {totalAvailable}, kiadni kívánt: {quantity}");

            decimal remainingToIssue = quantity;

            // Végigmegyünk az adagokon, és elkezdjük "leenni" róluk a mennyiséget
            foreach (var stock in stocks)
            {
                if (remainingToIssue <= 0) break;

                if (stock.Quantity >= remainingToIssue)
                {
                    stock.Quantity -= remainingToIssue;
                    remainingToIssue = 0;
                }
                else
                {
                    remainingToIssue -= stock.Quantity;
                    stock.Quantity = 0;
                }
            }

            var warehouseName = await _context.Warehouses
                    .Where(w => w.Id == warehouseId)
                    .Select(w => w.Name)
                    .FirstOrDefaultAsync();

            await _audit.LogAsync("PRODUCT_ISSUE", $"Termék Id: {productId}, Mennyiség: {quantity}, Raktár: {warehouseName}");

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
                    SubTotalValue = s.Quantity * s.Product.PurchasePrice
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