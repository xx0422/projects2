using ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AuditController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetLogs()
            => Ok(await _context.AuditLogs.OrderByDescending(x => x.Timestamp).Take(100).ToListAsync());
    }
}
