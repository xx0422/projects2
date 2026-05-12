using ERP.Models;

namespace ERP.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public Category Category { get; set; }
        public decimal CurrentAveragePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public int TotalStock { get; set; } 
        public UnitOfMeasure Unit { get; set; }
    }
}
