using HospitalAppointment.Data;
using HospitalAppointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Pages.Patient
{
    [Authorize] 
    public class MyAppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyAppointmentsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Appointment> Appointments { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            Appointments = await _context.Appointments
                .Include(a => a.Doctor) 
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == userId);

            if (appointment != null && appointment.StartTime > DateTime.Now)
            {
                appointment.Status = AppointmentStatus.Cancelled;

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}