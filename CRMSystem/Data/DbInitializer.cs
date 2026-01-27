// File: Data/DbInitializer.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CRMSystem.Models;

namespace CRMSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            // Seed roles
            string[] roleNames = { "Manager", "SalesRep" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed default Manager user from configuration
            var managerEmail = configuration["SeedAdmin:Email"] ?? "admin@crmsystem.com";
            var managerPassword = configuration["SeedAdmin:Password"] ?? "Admin@1234";

            var existingManager = await userManager.FindByEmailAsync(managerEmail);

            if (existingManager == null)
            {
                var managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(managerUser, managerPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create default Manager user: {errors}");
                }
            }
        }
    }
}
