using Microsoft.AspNetCore.Mvc;
using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]

    public class WarehouseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll()
        {
            return await _context.Warehouses.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Warehouse>> Create(Warehouse warehouse)
        {
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return Ok(warehouse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.StockItems)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null) return NotFound();

            if (warehouse.StockItems.Any(s => s.Quantity > 0))
            {
                return BadRequest("Nem törölhető olyan raktár, amiben van készlet!");
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
