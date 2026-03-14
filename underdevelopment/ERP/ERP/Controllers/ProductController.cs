using Microsoft.AspNetCore.Mvc;
using ERP.Models;
using ERP.Services;

namespace ERP.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Az elérési út: api/product
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // 1. Összes termék lekérése
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // 2. Egy termék lekérése ID alapján
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound("A termék nem található.");

            return Ok(product);
        }

        // 3. ÚJ TERMÉK LÉTREHOZÁSA 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message }); // Így 400-as hiba
            }
            catch (InvalidOperationException ex) // Pl. SKU hiba vagy szabályszegés
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Váratlan hiba történt: {ex.Message}");
            }
        }

        // 4. TERMÉK FRISSÍTÉSE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest("Az ID-k nem egyeznek.");

            try
            {
                await _productService.UpdateProductAsync(product);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Nincs ilyen termék.");
            }
        }

        // 5. TERMÉK TÖRLÉSE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}