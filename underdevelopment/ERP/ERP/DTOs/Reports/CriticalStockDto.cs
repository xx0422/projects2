namespace ERP.DTOs.Reports
{
    public class CriticalStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinRequired { get; set; }
    }
}