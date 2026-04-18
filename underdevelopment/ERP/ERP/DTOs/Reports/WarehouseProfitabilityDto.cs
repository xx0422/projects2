namespace ERP.DTOs.Reports
{
    public class WarehouseProfitabilityDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }  // Összes bevétel
        public decimal TotalCost { get; set; }     // Összes bekerülési érték
        public decimal Profit { get; set; }        // Haszon (Revenue - Cost)
        public decimal MarginPercentage { get; set; } // Árrés %
    }
}