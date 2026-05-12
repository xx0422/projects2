using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class ExpiredStockCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredStockCleanupService> _logger;

        public ExpiredStockCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredStockCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Selejtező robot elindult...");

            // Ez a ciklus fut a szerver élettartama alatt folyamatosan
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Megkeressük az összes lejárt terméket, amiből még van készlet
                    var expiredStocks = await context.StockItems
                        .Include(s => s.Product)
                        .Include(s => s.Warehouse)
                        .Where(s => s.ExpirationDate.HasValue
                                 && s.ExpirationDate.Value.Date < DateTime.UtcNow.Date
                                 && s.Quantity > 0)
                        .ToListAsync(stoppingToken);

                    if (expiredStocks.Any())
                    {
                        foreach (var stock in expiredStocks)
                        {
                            // 1. Létrehozunk egy naplóbejegyzést "Auto" felhasználóval
                            var log = new AuditLog
                            {
                                UserEmail = "Auto",
                                Action = "EXPIRED_STOCK_REMOVED",
                                Details = $"Automatikus selejtezés: {stock.Product.Name} ({stock.Quantity} {stock.Product.Unit}), Lejárat: {stock.ExpirationDate.Value.ToShortDateString()}, Raktár: {stock.Warehouse?.Name}",
                                Timestamp = DateTime.Now
                            };
                            context.AuditLogs.Add(log);

                            // 2. Levonjuk a készletet (vagy nullázzuk)
                            stock.Quantity = 0;

                            _logger.LogWarning($"Lejárt áru törölve: {stock.Product.Name}");
                        }
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}