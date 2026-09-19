using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace ARMTIS_Capstone_Project.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();


            // CREATE ROLES

            string[] roles =
            {
                "Admin",
                "Tenant"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            $"Failed to create role '{role}': " +
                            string.Join(", ",
                                roleResult.Errors.Select(e => e.Description))
                        );
                    }
                }
            }

            // CREATE ADMIN ACCOUNT

            string adminUsername = "admin";

            string adminPassword = "Admin@123";


            var adminUser =
                await userManager.FindByNameAsync(
                    adminUsername);


            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUsername,

                    Email = null,

                    EmailConfirmed = true
                };


                var result =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);


                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Failed to create admin account: " +
                        string.Join(", ",
                            result.Errors.Select(e =>
                                e.Description))
                    );
                }
            }


            // MAKE SURE ADMIN HAS ADMIN ROLE

            if (!await userManager.IsInRoleAsync(
                    adminUser,
                    "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        "Admin");


                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign Admin role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(e =>
                                e.Description))
                    );
                }
            }
        }
    }
}