using Microsoft.AspNetCore.Identity;
using ERP.Models; // Ellenőrizd, hogy a User osztályod melyik namespace-ben van

namespace ERP.Data
{
    public static class DbInitializer
    {
        private static ApplicationDbContext context; // Hozzáadva a context mező

        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>(); // Fontos: <User>!

            // 1. Szerepkörök létrehozása az Enumból (vagy kézzel)
            string[] roleNames = { "Admin", "StockKeeper", "Salesman", "Logistic" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Standard", Type = CategoryType.Standard },
                    new Category { Name = "Romlandó", Type = CategoryType.Perishable },
                    new Category { Name = "Veszélyes", Type = CategoryType.Hazardous },
                    new Category { Name = "Élő szervezet", Type = CategoryType.LivingOrganism }
                );
                await context.SaveChangesAsync();
            }

            // 2. Felhasználók adatai (Email, Jelszó, Szerepkör)
            var usersToSeed = new List<(string Email, string Password, UserRole Role, string UserName)>
            {
                ("admin@erp.hu", "ErpPassword123!", UserRole.Admin, "admin"),
                ("raktar@erp.hu", "Raktar123!", UserRole.StockKeeper, "raktaros"),
                ("ertekesito@erp.hu", "Sales123!", UserRole.Salesman, "elado"),
                ("logisztika@erp.hu", "Logistic123!", UserRole.Logistic, "logisztikus")
            };

            foreach (var userData in usersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        UserName = userData.UserName,
                        Email = userData.Email,
                        EmailConfirmed = true,
                        Role = userData.Role, // Itt mentjük el a saját Enumunkat
                        LastLogin = DateTime.Now
                    };

                    var result = await userManager.CreateAsync(newUser, userData.Password);
                    if (result.Succeeded)
                    {
                        // Az Identity belső szerepkör-kezelőjébe is hozzáadjuk
                        await userManager.AddToRoleAsync(newUser, userData.Role.ToString());
                    }
                }
            }
        }
    }
}