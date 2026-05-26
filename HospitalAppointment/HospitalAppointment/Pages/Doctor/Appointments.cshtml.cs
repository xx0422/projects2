using HospitalAppointment.Data;
using HospitalAppointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Pages.Doctor
{
    [Authorize(Roles = "Doctor")]
    public class AppointmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public IList<Appointment> Appointments { get; set; }

        public async Task OnGetAsync()
        {
            var doctorId = _userManager.GetUserId(User);

            Appointments = await _context.Appointments
                .Include(a => a.Patient) 
                .Where(a => a.DoctorId == doctorId && a.StartTime.Date == SelectedDate.Date)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        // Státusz változtatások
        public async Task<IActionResult> OnPostChangeStatusAsync(int id, AppointmentStatus status)
        {
            var doctorId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { SelectedDate = SelectedDate.ToString("yyyy-MM-dd") }); // nem mûködik
        }
    }
}