using ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PaymentStatus { Pending, Paid, Overdue, Cancelled }

public class Invoice
{
    public int Id { get; set; }

    // Jogi követelmény: Egyedi, sorszámozott számlaszám (pl. 2026/0001)
    [Required]
    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow; // Kiállítás dátuma
    public DateTime DueDate { get; set; }      // Fizetési határidő

    // Kapcsolatok
    public int OrderId { get; set; }           // Melyik rendelésről szól
    public virtual Order Order { get; set; } = null!;

    // Pénzügyi összesítők
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalNet { get; set; }      // Nettó végösszeg
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTax { get; set; }      // ÁFA összege
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGross { get; set; }    // Bruttó végösszeg

    public PaymentStatus Status { get; set; }

    // Számlatételek (Egy számlához több tétel tartozik)
    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}