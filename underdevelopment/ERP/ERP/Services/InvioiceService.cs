using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly OrderService _orderService; // Új függőség
        private readonly AuditService _audit;

        public InvoiceService(ApplicationDbContext context, IProductService productService, OrderService orderService, AuditService audit)
        {
            _context = context;
            _productService = productService;
            _orderService = orderService;
            _audit = audit;
        }

        public async Task<Invoice> CreateInvoiceAsync(int warehouseId, string customerName, List<InvoiceItemDto> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                // 1. Pesszimista lock: Zároljuk az Invoices táblát a generálás idejére (SQL Server)
                // Ez megakadályozza, hogy más kérések olvassák a sorszámot, amíg mi nem végeztünk.
                if (_context.Database.IsNpgsql())
                {
                    // PostgreSQL zárolás
                    await _context.Database.ExecuteSqlRawAsync("LOCK TABLE \"Invoices\" IN ACCESS EXCLUSIVE MODE");
                }
                else
                {
                    // SQL Server zárolás (Saját gép)
                    await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM Invoices WITH (TABLOCKX, HOLDLOCK)");
                }

                // 1. Számlaszám generálása
                int currentYear = DateTime.Now.Year;

                // Megkeressük az idei év legmagasabb sorszámát tartalmazó számlát
                var lastInvoice = await _context.Invoices
                    .Where(i => i.InvoiceNumber.StartsWith($"{currentYear}/"))
                    .OrderByDescending(i => i.InvoiceNumber) // Szöveges sorrend miatt a legmagasabbat hozza
                    .FirstOrDefaultAsync();

                int nextId = 1;

                if (lastInvoice != null)
                {
                    // Példa: "2024/0015" -> kivesszük a "/" utáni részt (0015)
                    string lastNumberPart = lastInvoice.InvoiceNumber.Split('/')[1];
                    if (int.TryParse(lastNumberPart, out int lastNumber))
                    {
                        nextId = lastNumber + 1;
                    }
                }

                string invoiceNumber = $"{currentYear}/{nextId.ToString("D4")}";

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    WarehouseId = warehouseId,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    Status = PaymentStatus.Pending,
                    CustomerName = customerName,
                    Items = new List<InvoiceItem>()
                };

                decimal totalNet = 0;
                decimal totalTax = 0;

                foreach (var itemDto in items)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null) throw new Exception($"Termék nem található: {itemDto.ProductId}");

                    // 2. Készlet levonása
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

                // 4. RENDELÉS GENERÁLÁSA A LOGISZTIKÁNAK
                // Ez az InvoiceService és OrderService közötti híd
                var order = await _orderService.CreateOrderFromInvoiceAsync(invoice);

                // Mivel az Order-t hozzáadtuk a context-hez, itt mentünk egyet, hogy legyen ID-ja
                await _context.SaveChangesAsync();

                // 5. ÖSSZEKÖTÉS
                invoice.Order = order;
                _context.Invoices.Add(invoice);

                await _audit.LogAsync("INVOICE_CREATE", $"Új számla: {invoiceNumber}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return invoice;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Számlázási és rendelési hiba: {ex.Message}");
            }
        }

        // A többi metódus (GetInvoicesByStatusAsync, ChangeInvoiceStatusAsync) változatlan maradhat
        public async Task<IEnumerable<object>> GetInvoicesByStatusAsync(PaymentStatus? status)
        {
            var query = _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Order)
                .AsQueryable();

            if (status.HasValue) query = query.Where(i => i.Status == status.Value);

            return await query
                .OrderByDescending(i => i.IssueDate)
                .Select(i => new {
                    i.Id,
                    i.InvoiceNumber,
                    i.CustomerName,
                    i.TotalGross,
                    i.Status,
                    StatusName = i.Status.ToString(),
                    i.IssueDate,
                    Order = i.Order != null ? new { Status = i.Order.Status.ToString() } : null
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
                if (!invoice.WarehouseId.HasValue)
                {
                    throw new Exception("A számla raktára már nem létezik, a készlet visszavételezése sikertelen!");
                }

                foreach (var item in invoice.Items)
                {
                    await _productService.ProcessStockReceiptAsync(item.ProductId, invoice.WarehouseId.Value, item.Quantity, item.UnitPrice);
                }
            }
            invoice.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    }
    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; } = 27;
    }
}