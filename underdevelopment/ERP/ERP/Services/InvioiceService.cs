using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public InvoiceService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<Invoice> CreateInvoiceAsync(int warehouseId, string customerName, List<InvoiceItemDto> items)
        {
            // Tranzakció indítása
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Számlaszám generálása (évszám + következő sorszám)
                var lastInvoiceCount = await _context.Invoices.CountAsync();
                var invoiceNumber = $"{DateTime.Now.Year}/{(lastInvoiceCount + 1):D4}";

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    WarehouseId = warehouseId,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14), // Alapértelmezett 14 nap
                    Status = PaymentStatus.Pending,
                    CustomerName = customerName
                };

                decimal totalNet = 0;
                decimal totalTax = 0;

                foreach (var itemDto in items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null) throw new Exception($"Termék nem található: {itemDto.ProductId}");

                    // 2. Készlet levonása a konkrét raktárból
                    await _productService.ProcessStockIssueAsync(itemDto.ProductId, warehouseId, itemDto.Quantity);

                    // 3. Tétel kiszámítása
                    var lineNet = itemDto.Quantity * itemDto.UnitPrice;
                    var lineTax = lineNet * (itemDto.TaxRate / 100);

                    var invoiceItem = new InvoiceItem
                    {
                        ProductId = itemDto.ProductId,
                        ProductName = product.Name, 
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        BuyingPrice = product.CurrentAveragePrice,
                        TaxRate = itemDto.TaxRate,
                        LineTotal = lineNet + lineTax
                    };

                    invoice.Items.Add(invoiceItem);
                    totalNet += lineNet;
                    totalTax += lineTax;
                }

                invoice.TotalNet = totalNet;
                invoice.TotalTax = totalTax;
                invoice.TotalGross = totalNet + totalTax;

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Minden sikerült -> Véglegesítés
                await transaction.CommitAsync();
                return invoice;
            }
            catch (Exception ex)
            {
                // Hiba esetén minden módosítást visszavonunk (Készletlevonást is!)
                await transaction.RollbackAsync();
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception($"Számlázási hiba: {ex.Message}");
            }
        }

        public async Task<IEnumerable<object>> GetInvoicesByStatusAsync(PaymentStatus? status)
        {
            var query = _context.Invoices.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            return await query
                .OrderByDescending(i => i.IssueDate)
                .Select(i => new {
                    i.Id,
                    i.InvoiceNumber,
                    i.CustomerName,
                    i.TotalGross,
                    i.Status,
                    StatusName = i.Status.ToString(), 
                    i.IssueDate
                })
                .ToListAsync();
        }

        public async Task ChangeInvoiceStatusAsync(int invoiceId, PaymentStatus newStatus)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) throw new Exception("Számla nem található.");

            if (newStatus == PaymentStatus.Cancelled && invoice.Status != PaymentStatus.Cancelled)
            {
                foreach (var item in invoice.Items)
                {
                    await _productService.ProcessStockReceiptAsync(item.ProductId, invoice.WarehouseId, item.Quantity, item.UnitPrice);
                }
            }
            else if (invoice.Status == PaymentStatus.Cancelled && newStatus != PaymentStatus.Cancelled)
            {
                throw new Exception("Sztornózott számlát nem lehet újra aktiválni, készíts újat!");
            }

            invoice.Status = newStatus;
            await _context.SaveChangesAsync();
        }

    }

    // Segéd-osztály a Swagger beküldéshez
    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 27; // Alapértelmezett magyar ÁFA
    }
}