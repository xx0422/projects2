using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace ERP.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string details)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            // Végignézzük a leggyakoribb JWT Claim-eket, ahova az azonosító kerülhetett
            string userEmail = user?.FindFirst(ClaimTypes.Email)?.Value
                            ?? user?.FindFirst(ClaimTypes.Name)?.Value
                            ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? user?.FindFirst("email")?.Value
                            ?? user?.FindFirst("sub")?.Value
                            ?? user?.Identity?.Name
                            ?? "Rendszer"; // Ha végképp nincs token (pl. háttérfolyamat)

            var log = new AuditLog
            {
                Action = action,
                Details = details,
                UserEmail = userEmail,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
