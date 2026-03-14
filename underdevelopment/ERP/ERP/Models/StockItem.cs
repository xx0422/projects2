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
        public decimal Quantity { get; set; } // Jelenlegi készlet

        [JsonIgnore]
        public virtual Product? Product { get; set; }

        // Függvények
        public void AddStock(decimal amount) { Quantity += amount; }
        public void RemoveStock(decimal amount) { Quantity -= amount; }
    }
}
