using HospitalAppointment.Data;
using HospitalAppointment.Models;
using HospitalAppointment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HospitalAppointment.Pages.Patient
{
    [Authorize]
    public class BookModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppointmentService _appointmentService;

        public BookModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAppointmentService appointmentService)
        {
            _context = context;
            _userManager = userManager;
            _appointmentService = appointmentService;
        }

        public void OnGet() { }

        // Visszaadja az orvosokat egy adott szakterület alapján
        public async Task<JsonResult> OnGetDoctorsBySpecialtyAsync(string specialty)
        {
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            var filteredDoctors = doctors
                .Where(d => d.Specialty == specialty)
                .Select(d => new { id = d.Id, name = d.FullName })
                .ToList();

            return new JsonResult(filteredDoctors);
        }

        // Visszaadja a következõ 30 napból azokat, amikor az orvos rendel
        public async Task<JsonResult> OnGetAvailableDatesAsync(string doctorId)
        {
            var schedules = await _context.DoctorSchedules.Where(s => s.DoctorId == doctorId).ToListAsync();
            var workingDays = schedules.Select(s => s.Day).ToList();

            var availableDates = new List<object>();
            var culture = new CultureInfo("hu-HU");

            for (int i = 1; i <= 30; i++) 
            {
                var date = DateTime.Today.AddDays(i);
                if (workingDays.Contains(date.DayOfWeek))
                {
                    availableDates.Add(new
                    {
                        dateValue = date.ToString("yyyy-MM-dd"),
                        displayTitle = date.ToString("yyyy. MM. dd. - dddd", culture)
                    });
                }
            }

            return new JsonResult(availableDates);
        }

        // Visszaadja a 15 perces szabad blokkokat a kiválasztott napon
        public async Task<JsonResult> OnGetTimeSlotsAsync(string doctorId, DateTime date)
        {
            // Lekérjük az összes beosztást az adott napra
            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.Day == date.DayOfWeek)
                .ToListAsync();

            var availableSlots = new List<string>();

            foreach (var schedule in schedules)
            {
                var currentTime = schedule.StartTime;

                while (currentTime.Add(TimeSpan.FromMinutes(15)) <= schedule.EndTime)
                {
                    var slotStart = currentTime;
                    var slotEnd = currentTime.Add(TimeSpan.FromMinutes(15));

                    DateTime fullStartTime = date.Add(slotStart);
                    DateTime fullEndTime = date.Add(slotEnd);

                    bool hasConflict = await _context.Appointments
                        .AnyAsync(a => a.DoctorId == doctorId &&
                                       a.Status != AppointmentStatus.Cancelled &&
                                       a.StartTime < fullEndTime &&
                                       a.EndTime > fullStartTime);

                    if (!hasConflict)
                    {
                        availableSlots.Add(slotStart.ToString(@"hh\:mm"));
                    }

                    currentTime = slotEnd;
                }
            }
            availableSlots.Sort();

            return new JsonResult(availableSlots);
        }

        // Foglalás feldolgozása
        public async Task<IActionResult> OnPostAsync(string doctorId, DateTime date, string timeSlot)
        {
            var patientId = _userManager.GetUserId(User);
            var startTimeSpan = TimeSpan.Parse(timeSlot);
            var endTimeSpan = startTimeSpan.Add(TimeSpan.FromMinutes(15)); 

            DateTime fullStart = date.Add(startTimeSpan);
            DateTime fullEnd = date.Add(endTimeSpan);

            bool success = await _appointmentService.TryBookAppointmentAsync(patientId, doctorId, fullStart, fullEnd);

            if (success) TempData["Message"] = "Sikeres idõpontfoglalás!";
            else TempData["Error"] = "Hiba! Az idõpont már foglalt.";

            return RedirectToPage();
        }
    }
}