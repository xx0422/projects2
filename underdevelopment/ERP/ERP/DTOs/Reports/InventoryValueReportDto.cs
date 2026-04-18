namespace ERP.DTOs.Reports
{
    public class InventoryValueReportDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public decimal TotalItems { get; set; }
        public decimal TotalValue { get; set; }
    }
}
