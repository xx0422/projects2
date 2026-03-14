namespace ERP.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal UnitPriceAtTimeOfOrder { get; set; }
    }
}
