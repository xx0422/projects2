using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class StockItem
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }
        public decimal Quantity { get; set; } // Jelenlegi készlet
        public DateTime? ExpirationDate { get; set; }

        public virtual Product? Product { get; set; }

        
    }
}
