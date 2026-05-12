using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class LogisticsService
    {
        private readonly ApplicationDbContext _context;

        public LogisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Új szállítmány indítása egy rendeléshez
        public async Task<Shipment> CreateShipmentAsync(int orderId, string carrier)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) throw new Exception("Rendelés nem található!");

            string trackingNumber = await GenerateTrackingNumber();

            var shipment = new Shipment
            {
                OrderId = orderId,
                CarrierName = carrier,
                TrackingNumber = trackingNumber,
                DispatchDate = DateTime.Now,
                Status = ShipmentStatus.Dispatched
            };

            // A rendelés státuszát is frissítjük "InTransit"-ra
            order.Status = OrderStatus.InTransit;

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
            return shipment;
        }

        private async Task<string> GenerateTrackingNumber()
        {
            int year = DateTime.Now.Year;
            var count = await _context.Shipments.CountAsync(s => s.DispatchDate.Year == year) + 1;
            // Formátum: TRK-2026-0001
            return $"TRK-{year}-{count:D4}";
        }

        // 2. Szállítási státusz frissítése (pl. megérkezett)
        public async Task UpdateShipmentStatusAsync(int shipmentId, ShipmentStatus newStatus)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);

            if (shipment == null) return;

            shipment.Status = newStatus;

            // Ha a szállítmány megérkezett, a rendelést is lezárjuk
            if (newStatus == ShipmentStatus.Delivered)
            {
                shipment.Order.Status = OrderStatus.Delivered;
            }

            await _context.SaveChangesAsync();
        }
    }
}