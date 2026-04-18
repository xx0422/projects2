using ERP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ERP.Data;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceService _invoiceService;
    private readonly ApplicationDbContext _context;

    public InvoiceController(InvoiceService invoiceService, ApplicationDbContext context)
    {
        _invoiceService = invoiceService;
        _context = context;

    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice(int warehouseId, string customerName, List<InvoiceItemDto> items)
    {
        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(warehouseId, customerName, items);
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        // Be kell emelnünk az Items-eket is, különben üres lesz a PDF táblázata!
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound("A számla nem található.");

        // QuestPDF licenc beállítása (Kötelező a legújabb verziókban)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = new InvoiceDocument(invoice);
        byte[] pdfData = document.GeneratePdf();

        string fileName = $"Szamla_{invoice.InvoiceNumber.Replace("/", "-")}.pdf";
        return File(pdfData, "application/pdf", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(PaymentStatus? status)
    {
        var invoices = await _invoiceService.GetInvoicesByStatusAsync(status);
        return Ok(invoices);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, PaymentStatus newStatus)
    {
        try
        {
            await _invoiceService.ChangeInvoiceStatusAsync(id, newStatus);
            return Ok(new { message = $"A számla státusza {newStatus}-ra frissült, a készletmozgás lefutott." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}