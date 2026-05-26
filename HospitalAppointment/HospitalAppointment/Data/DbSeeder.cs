using HospitalAppointment.Models;
using Microsoft.AspNetCore.Identity;

namespace HospitalAppointment.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Identity szolgáltatások (DI konténerből) 
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Doctor", "Patient" };

            // Létrehozzuk a szerepköröket
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@hospital.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Fő Adminisztrátor",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Password1!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}