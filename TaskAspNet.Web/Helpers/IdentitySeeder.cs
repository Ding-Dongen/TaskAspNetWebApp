
using Microsoft.AspNetCore.Identity;
using TaskAspNet.Data.EntityIdentity;

namespace TaskAspNet.Web.Infrastructure;

// Created with the help of ChatGPT4.5
// Seeds the SuperAdmin, Admin, and User roles if they don’t exist
// Ensures a SuperAdmin user account exists with a known email and password
// Confirms the SuperAdmin user has the SuperAdmin role

public static class IdentitySeeder
{
    public static async Task SeedSuperAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roleNames = { "SuperAdmin", "Admin", "User" };
        foreach (var role in roleNames)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        const string email = "superadmin@example.com";
        const string password = "SuperAdmin123!";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin"
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception("Failed to create super-admin: " +
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
            await userManager.AddToRoleAsync(user, "SuperAdmin");
    }
}
