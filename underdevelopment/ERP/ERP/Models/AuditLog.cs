namespace ERP.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string UserEmail { get; set; } = "Rendszer";
        public string Action { get; set; } = string.Empty; // LOGIN, STOCK_RECEIPT, INVOICE_CREATE...
        public string Details { get; set; } = string.Empty;
    }
}
