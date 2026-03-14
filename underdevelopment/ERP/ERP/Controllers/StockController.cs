using ERP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IProductService _productService;

        public StockController(IProductService productService)
        {
            _productService = productService;
        }

        // Bevételezés végpont
        [HttpPost("receipt")]
        public async Task<IActionResult> ProcessReceipt(int productId, decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0) return BadRequest("A mennyiségnek pozitívnak kell lennie.");

            try
            {
                await _productService.ProcessStockReceiptAsync(productId, quantity, unitPrice);
                return Ok(new { message = "Bevételezés sikeres, átlagár frissítve." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("issue")]
        public async Task<IActionResult> ProcessIssue(int productId, decimal quantity)
        {
            if (quantity <= 0) return BadRequest("A mennyiségnek pozitívnak kell lennie.");
            try
            {
                await _productService.ProcessStockIssueAsync(productId, quantity);
                return Ok(new { message = "Kiadás sikeres." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            try
            {
                var inventory = await _productService.GetInventorySummaryAsync();
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}