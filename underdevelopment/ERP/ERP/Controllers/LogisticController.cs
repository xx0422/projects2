using ERP.Data;
using ERP.Models;
using ERP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LogisticsController : ControllerBase
{
    private readonly LogisticsService _logisticsService;
    private readonly ApplicationDbContext _context;

    public LogisticsController(LogisticsService logisticsService, ApplicationDbContext context)
    {
        _logisticsService = logisticsService;
        _context = context;
    }

    // Szállításra váró rendelések listája
    [HttpGet("pending-orders")]
    public async Task<IActionResult> GetPendingOrders()
    {
        var pendingInvoices = await _context.Invoices
            .Include(i => i.Order)
            .Where(i => i.Order != null && i.Order.Status == OrderStatus.Processing)
            .Select(i => new {
                OrderId = i.OrderId,
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                OrderDate = i.Order.OrderDate,
                CustomerName = i.CustomerName, 
                Status = i.Order.Status.ToString()
            })
            .ToListAsync();

        return Ok(pendingInvoices);
    }

    [HttpPost("dispatch")]
    public async Task<IActionResult> DispatchOrder(int orderId, string carrier)
    {
        try
        {
            var shipment = await _logisticsService.CreateShipmentAsync(orderId, carrier);
            return Ok(shipment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("in-transit-orders")]
    public async Task<IActionResult> GetInTransitOrders()
    {
        var inTransitInvoices = await _context.Invoices
            .Include(i => i.Order) 
                .ThenInclude(o => o.Shipment) // A rendelésen keresztül beemeljük a szállítmányt
            .Where(i => i.Order != null && i.Order.Status == OrderStatus.InTransit)
            .Select(i => new {
                OrderId = i.OrderId,
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerName = i.CustomerName, 
                Carrier = i.Order.Shipment != null ? i.Order.Shipment.CarrierName : "Ismeretlen",
                DispatchDate = i.Order.Shipment != null ? i.Order.Shipment.DispatchDate : (DateTime?)null
            })
            .ToListAsync();

        return Ok(inTransitInvoices);
    }

    [HttpPost("mark-as-delivered")]
    public async Task<IActionResult> MarkAsDelivered(int orderId)
    {
        // A FindAsync a Context által követett (tracked) példányt adja vissza
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
            return NotFound("Rendelés nem található.");

        // Csak akkor tudjuk lezárni, ha már úton volt
        if (order.Status != OrderStatus.InTransit)
        {
            return BadRequest($"A rendelést nem lehet lezárni, mert jelenleg {order.Status} állapotban van.");
        }

        // Átállítjuk a státuszt a végállapotra
        order.Status = OrderStatus.Delivered;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Rendelés sikeresen lezárva (Kiszállítva)." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Hiba történt a mentés során: " + ex.Message });
        }
    }


}