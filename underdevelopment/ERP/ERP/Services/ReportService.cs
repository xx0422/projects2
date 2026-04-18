using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;
using ERP.DTOs.Reports;

namespace ERP.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<object> GetInventoryValueReportAsync()
        {
            var data = await _context.StockItems
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .GroupBy(s => s.Warehouse.Name)
                .Select(group => new InventoryValueReportDto{
                    WarehouseName = group.Key,
                    TotalItems = group.Sum(s => s.Quantity),
                    // Készletérték = Mennyiség * Aktuális átlagár
                    TotalValue = group.Sum(s => s.Quantity * s.Product.CurrentAveragePrice)
                })
                .ToListAsync();

            return new
            {
                GeneratedAt = DateTime.Now,
                Details = data,
                GrandTotalValue = data.Sum(d => d.TotalValue)
            };
        }

        public async Task<object> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _context.Invoices
                .Where(i => i.IssueDate >= startDate && i.IssueDate <= endDate && i.Status != PaymentStatus.Cancelled)
                .GroupBy(i => i.IssueDate.Date) // Naponkénti csoportosítás
                .Select(group => new SalesReportDto {
                    Date = group.Key,
                    InvoiceCount = group.Count(),
                    DailyRevenue = group.Sum(i => i.TotalGross)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            return new
            {
                Period = $"{startDate:yyyy.MM.dd} - {endDate:yyyy.MM.dd}",
                DailyBreakdown = sales,
                TotalPeriodRevenue = sales.Sum(s => s.DailyRevenue)
            };
        }

        public async Task<object> GetAbcAnalysisReportAsync()
        {
            var salesData = await _context.InvoiceItems
                .Where(ii => ii.Invoice.Status != PaymentStatus.Cancelled)
                .GroupBy(ii => ii.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    TotalRevenue = g.Sum(ii => ii.LineTotal)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            decimal grandTotal = salesData.Sum(x => x.TotalRevenue);
            decimal cumulativeRevenue = 0;

            var result = salesData.Select(x => {
                cumulativeRevenue += x.TotalRevenue;
                decimal percentage = (cumulativeRevenue / grandTotal) * 100;

                string category = percentage <= 80 ? "A" : (percentage <= 95 ? "B" : "C");

                return new AbcAnalysisDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    TotalRevenue = x.TotalRevenue,
                    CumulativePercentage = Math.Round(percentage, 2),
                    Category = category
                };
            }).ToList();

            return result;
        }

        public async Task<object> GetWarehouseProfitabilityReportAsync()
        {
            var report = await _context.Invoices
                .Where(i => i.Status != PaymentStatus.Cancelled)
                .SelectMany(i => i.Items.Select(item => new {
                    WarehouseName = i.Warehouse.Name,
                    Revenue = item.Quantity * item.UnitPrice, // Nettó bevétel
                    Cost = item.Quantity * item.BuyingPrice   // Nettó önköltség
                }))
                .GroupBy(x => x.WarehouseName)
                .Select(group => new WarehouseProfitabilityDto {
                    WarehouseName = group.Key,
                    TotalRevenue = group.Sum(x => x.Revenue),
                    TotalCost = group.Sum(x => x.Cost),
                    // Profit = Bevétel - Önköltség
                    Profit = group.Sum(x => x.Revenue - x.Cost),
                    // Árrés % (Profit / Bevétel * 100)
                    MarginPercentage = group.Sum(x => x.Revenue) != 0
                        ? Math.Round(group.Sum(x => x.Revenue - x.Cost) / group.Sum(x => x.Revenue) * 100, 2)
                        : 0
                })
                .ToListAsync();

            return report;
        }

        public async Task<object> GetCriticalStockReportAsync()
        {
            var report = await _context.StockItems
                .Include(p => p.Warehouse)
                .Include(p => p.Product)
                .Select(p => new CriticalStockDto {
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    WarehouseName = p.Warehouse.Name,
                    SKU = p.Product.SKU,
                    CurrentStock = p.Quantity,
                    MinRequired = 5
                })
                .Where(x => x.CurrentStock <= x.MinRequired)
                .OrderBy(x => x.WarehouseName)
                .ThenBy(x => x.CurrentStock)
                .ToListAsync();

            return report;
        }
    }
}
