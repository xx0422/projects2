using ERP.DTOs.Reports;
using ERP.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("Inventory Value")]
        [ProducesResponseType(typeof(IEnumerable<InventoryValueReportDto>), 200)]
        public async Task<IActionResult> GetInventoryVaLueReport()
        {
            var report = await _reportService.GetInventoryValueReportAsync();
            return Ok(report);
        }

        [HttpGet("Sales Report")]
        [ProducesResponseType(typeof(IEnumerable<SalesReportDto>), 200)]
        public async Task<IActionResult> GetSalesReport(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GetSalesReportAsync(startDate, endDate);
            return Ok(report);
        }

        [HttpGet("abc-analysis")]
        [ProducesResponseType(typeof(IEnumerable<AbcAnalysisDto>), 200)]
        public async Task<IActionResult> GetAbcAnalysis()
        {
            var report = await _reportService.GetAbcAnalysisReportAsync();
            return Ok(report);
        }

        [HttpGet("warehouse-profitability")]
        [ProducesResponseType(typeof(IEnumerable<WarehouseProfitabilityDto>), 200)]
        public async Task<IActionResult> GetProfitability()
        {
            var report = await _reportService.GetWarehouseProfitabilityReportAsync();
            return Ok(report);
        }

        [HttpGet("critical-stock")]
        [ProducesResponseType(typeof(IEnumerable<CriticalStockDto>), 200)]
        public async Task<IActionResult> GetCriticalStock()
        {
            var report = await _reportService.GetCriticalStockReportAsync();
            return Ok(report);
        }
    }
}
