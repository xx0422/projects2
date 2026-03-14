namespace ERP.Models
{
    public enum ShipmentStatus { Pending, Dispatched, InTransit, Delivered, Cancelled }
    public class Shipment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public string CarrierName { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime DispatchDate { get; set; }
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    }
}
