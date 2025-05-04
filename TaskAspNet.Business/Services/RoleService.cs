
using Microsoft.AspNetCore.Identity;
using TaskAspNet.Data.EntityIdentity;

namespace TaskAspNet.Business.Services;

public class RoleService(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
{

    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<bool> AssignUserToAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            Console.WriteLine("User not found.");
            return false;
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!roleResult.Succeeded)
            {
                Console.WriteLine("Failed to create 'Admin' role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return false;
            }
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            Console.WriteLine("User is already in 'Admin' role.");
            return false;
        }

        if (await _userManager.IsInRoleAsync(user, "User"))
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, "User");
            if (!removeResult.Succeeded)
            {
                Console.WriteLine("Failed to remove 'User' role: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                return false;
            }
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (!result.Succeeded)
        {
            Console.WriteLine("Failed to add user to 'Admin' role: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        return true;
    }


    public async Task<bool> RemoveAdminRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false; 

        
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, "Admin");

            if (!removeResult.Succeeded)
            {
                return false; 
            }
        }

        
        if (!await _userManager.IsInRoleAsync(user, "User"))
        {
            var addResult = await _userManager.AddToRoleAsync(user, "User");
            return addResult.Succeeded;
        }

        return true; 
    }

}


