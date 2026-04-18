namespace ERP.DTOs.Reports
{
    public class SalesReportDto
    {
        public DateTime Date { get; set; }
        public int InvoiceCount { get; set; }
        public decimal DailyRevenue { get; set; }
    }
}
