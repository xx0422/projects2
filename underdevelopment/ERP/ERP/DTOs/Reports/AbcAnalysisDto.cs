namespace ERP.DTOs.Reports
{
    public class AbcAnalysisDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal CumulativePercentage { get; set; }
        public string Category { get; set; } = string.Empty; // "A", "B" vagy "C"
    }
}