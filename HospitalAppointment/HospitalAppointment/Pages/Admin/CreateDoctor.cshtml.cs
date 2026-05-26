using HospitalAppointment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateDoctorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateDoctorModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Ide kötjük be az ûrlapról érkezõ adatokat (Model Binding)
        [BindProperty]
        public DoctorInputModel Input { get; set; }

        public class DoctorInputModel
        {
            [Required(ErrorMessage = "A név megadása kötelezõ.")]
            [Display(Name = "Orvos teljes neve")]
            public string ? FullName { get; set; }

            [Required(ErrorMessage = "A szakterület kötelezõ.")]
            [Display(Name = "Szakterület")]
            public string ? Specialty { get; set; }

            [Required(ErrorMessage = "Az email megadása kötelezõ.")]
            [EmailAddress]
            [Display(Name = "Email cím")]
            public string ? Email { get; set; }

            [Required(ErrorMessage = "A jelszó kötelezõ.")]
            [DataType(DataType.Password)]
            [Display(Name = "Jelszó (ideiglenes)")]
            public string ? Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            // Létrehozzuk az új felhasználó objektumot
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                Specialty = Input.Specialty, 
                EmailConfirmed = true
            };

            // Mentés az adatbázisba 
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Doctor");

                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}