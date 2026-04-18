using ERP.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty; // Mentéskor ez "befagy" a termék nevére, hogy később is lássuk, mi volt a tétel
    public int ProductId { get; set; } // Kell a készletlevonáshoz!
    public virtual Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BuyingPrice { get; set; }   // Beszerzési ár 
    public decimal UnitPrice { get; set; }    // Egységár
    public decimal TaxRate { get; set; }      // 27%-os ÁFA

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }    // (Quantity * UnitPrice) + Tax
}