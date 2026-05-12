namespace ERP.Models
{
    public enum OrderStatus { Processing, InTransit, OnWay, Delivered, Cancelled }
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Processing;
        public int CustomerId { get; set; } 
        public string? CustomerName { get; set; }
        public virtual Shipment? Shipment { get; set; }
        public virtual Invoice? Invoice { get; set; }

        // A rendelés tételei (egy rendelés -> több tétel)
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>(); // 1:N kapcsolat - egy orderhez több orderitem, EF-ben virtual és ICollection a lazy loading miatt 
    }
}
