using HospitalAppointment.Data;
using HospitalAppointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Pages.Doctor
{
    [Authorize(Roles = "Doctor")]
    public class ScheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public ScheduleInputModel Input { get; set; }

        // Itt fogjuk tárolni és a felületre küldeni az orvos már meglévõ beosztásait
        public IList<DoctorSchedule> MySchedules { get; set; }

        public class ScheduleInputModel
        {
            [Required(ErrorMessage = "Válassz napot!")]
            [Display(Name = "A hét napja")]
            public DayOfWeek Day { get; set; }

            [Required(ErrorMessage = "Kezdési idõ kötelezõ!")]
            [Display(Name = "Kezdés (pl. 08:00)")]
            [DataType(DataType.Time)]
            public TimeSpan StartTime { get; set; }

            [Required(ErrorMessage = "Befejezési idõ kötelezõ!")]
            [Display(Name = "Befejezés (pl. 12:00)")]
            [DataType(DataType.Time)]
            public TimeSpan EndTime { get; set; }
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            MySchedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == userId)
                .OrderBy(s => s.Day).ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Ha hiba van, újra be kell tölteni a listát
                return Page();
            }

            if (Input.StartTime >= Input.EndTime)
            {
                ModelState.AddModelError(string.Empty, "A befejezési idõnek késõbb kell lennie, mint a kezdési idõnek!");
                await OnGetAsync();
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            // Új beosztás objektum
            var schedule = new DoctorSchedule
            {
                DoctorId = userId,
                Day = Input.Day,
                StartTime = Input.StartTime,
                EndTime = Input.EndTime
            };

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            var scheduleToDelete = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == userId);

            if (scheduleToDelete != null)
            {
                _context.DoctorSchedules.Remove(scheduleToDelete);
                await _context.SaveChangesAsync(); 
            }

            return RedirectToPage();
        }
    }
}