using ERP.Data;
using ERP.Models;
using ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ERP.Controllers
{
    [Authorize]
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

        // 1. SZÁMLA LÉTREHOZÁSA (ÉS AUTOMATIKUS RENDELÉS GENERÁLÁS)
        [Authorize(Roles = "Salesman, Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest("A számla adatai vagy tételei hiányoznak.");
            }

            try
            {
                // A request objektumból szedjük ki az adatokat
                var invoice = await _invoiceService.CreateInvoiceAsync(
                    request.WarehouseId,
                    request.CustomerName,
                    request.Items);

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // 2. SZÁMLA PDF GENERÁLÁSA
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound("A számla nem található.");

            QuestPDF.Settings.License = LicenseType.Community;

            var document = new InvoiceDocument(invoice);
            byte[] pdfData = document.GeneratePdf();

            string fileName = $"Szamla_{invoice.InvoiceNumber.Replace("/", "-")}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }

        // 3. SZÁMLÁK LISTÁZÁSA STÁTUSZ ALAPJÁN
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaymentStatus? status)
        {
            var invoices = await _invoiceService.GetInvoicesByStatusAsync(status);
            return Ok(invoices);
        }

        // 4. SZÁMLA STÁTUSZÁNAK MÓDOSÍTÁSA (PL. FIZETVE)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] PaymentStatus newStatus)
        {
            try
            {
                await _invoiceService.ChangeInvoiceStatusAsync(id, newStatus);
                return Ok(new { message = $"A számla státusza sikeresen frissítve: {newStatus}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Segéd-objektum a kérés fogadásához (Request DTO)
    public class CreateInvoiceRequest
    {
        public int WarehouseId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }
}