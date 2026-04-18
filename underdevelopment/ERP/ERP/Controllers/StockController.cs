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
        public async Task<IActionResult> ProcessReceipt(int productId, int warehouseId, decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0) return BadRequest("A mennyiségnek pozitívnak kell lennie.");

            try
            {
                await _productService.ProcessStockReceiptAsync(productId, warehouseId, quantity, unitPrice);
                return Ok(new { message = "Bevételezés sikeres, átlagár frissítve." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("issue")]
        public async Task<IActionResult> ProcessIssue(int productId, int warehouseId, decimal quantity)
        {
            if (quantity <= 0) return BadRequest("A mennyiségnek pozitívnak kell lennie.");
            try
            {
                await _productService.ProcessStockIssueAsync(productId, warehouseId, quantity);
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

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock(int productId, int fromWarehouseId, int toWarehouseId, decimal quantity)
        {
            if (quantity <= 0) return BadRequest("A mennyiségnek pozitívnak kell lennie.");

            try
            {
                await _productService.TransferStockAsync(productId, fromWarehouseId, toWarehouseId, quantity);
                return Ok(new { message = $"Sikeres átvezetés: {quantity} db termék mozgatva." });
            }
            catch (Exception ex)
            {
                // Itt kapjuk el, ha nincs elég készlet vagy nem létezik a raktár
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetStockByWarehouse(int warehouseId)
        {
            var stock = await _productService.GetStockByWarehouseAsync(warehouseId);
            return Ok(stock);
        }

        [HttpGet("where-is/{productId}")]
        public async Task<IActionResult> GetProductLocations(int productId, int quantity)
        {
            var locations = await _productService.GetProductLocationsAsync(productId, quantity);

            if (!locations.Any())
                return NotFound(new { message = "Ez a termék egyik raktárban sem található meg." });

            return Ok(locations);
        }
    }
}