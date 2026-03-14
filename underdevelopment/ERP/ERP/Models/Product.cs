using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public enum UnitOfMeasure
    {
        db,
        kg,
        l,
        karton,

    }
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string SKU { get; set; }  //Egyedi termékazonosító vonalkód helyett

        public string? Description { get; set; } = string.Empty;

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Required]
        public decimal MinimumOrderQuantity { get; set; } = 1;

        public UnitOfMeasure Unit { get; set; }

        // Átlagárazás alapja: ezt a Service Layer fogja frissíteni minden bevételezésnél
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAveragePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // Ez a legutóbbi beszerzési ár, amit a Service Layer frissít minden bevételezésnél

        // Idegen kulcs a kategóriához
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        // Speciális mezők (Nullable, mert nem minden termékhez kellenek) 

        public DateTime? ExpirationDate { get; set; } // Csak romlandóhoz

        public string? StorageConditions { get; set; } // Pl. "Fagyasztva tárolandó"

        public string? SafetyDocumentURL { get; set; } // Veszélyes árukhoz

        public string? FoodSafetyCertificateId { get; set; } // Élő élelmiszerekhez kötelező élelmiszer-biztonsági igazolás


        public void UpdateAverageCost(decimal newBatchQuantity, decimal newBatchPrice) { /* ... */ }
        public decimal CalculateInventoryValue(decimal totalStock) { return 0; }


    }
}
