using HospitalAppointment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "A név megadása kötelezõ!")]
            [Display(Name = "Teljes Név")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Az email kötelezõ!")]
            [EmailAddress]
            [Display(Name = "Email cím")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A jelszó kötelezõ!")]
            [StringLength(100, ErrorMessage = "A jelszónak legalább {6} karakter hosszúnak kell lennie.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Jelszó")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Jelszó megerõsítése")]
            [Compare("Password", ErrorMessage = "A két jelszó nem egyezik meg.")]
            public string ConfirmPassword { get; set; }
        }
        // elmenti a kívánt visszatérési URL-t, hogy a regisztráció után oda irányíthassuk a felhasználót
        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // Alapértelmezett visszatérés a fõoldalra

            if (ModelState.IsValid)
            {
                // Létrehozzuk az új felhasználót a bekért adatokkal
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    EmailConfirmed = true    
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Automatikusan beletesszük  Patient szerepkörbe
                    await _userManager.AddToRoleAsync(user, "Patient");

                    // Bejelentkeztetjük
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Visszaküldjük a fõoldali útvonalválasztónkhoz
                    return LocalRedirect(returnUrl);
                }

                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}