using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HospitalAppointment.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToPage("/Admin/CreateDoctor");

                if (User.IsInRole("Doctor")) return RedirectToPage("/Doctor/Schedule");

                if (User.IsInRole("Patient")) return RedirectToPage("/Patient/Book");
            }

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }
}