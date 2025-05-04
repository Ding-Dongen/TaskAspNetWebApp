using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.EntityIdentity;

namespace TaskAspNet.Web.Controllers;
public class AdminController(IMemberService memberService, UserManager<AppUser> userManager, RoleService roleService, ILogger<AdminController> logger) : Controller
{
    private readonly IMemberService _memberService = memberService;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleService _roleService = roleService;
    private readonly ILogger<AdminController> _logger = logger;



    public IActionResult Index()
    {
        try
        {
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Index view.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize(Roles = "Admin, User, SuperAdmin")]
    public async Task<IActionResult> Members()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync();
            return View(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members.");
            return StatusCode(500, "An unexpected error occurred while retrieving members.");
        }
    }

    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> ManageUsers()
    {
        try
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var freshUser = await _userManager.FindByIdAsync(user.Id);

                var roles = await _userManager.GetRolesAsync(freshUser);
                var userRole = roles.Contains("SuperAdmin") ? "SuperAdmin" : roles.Contains("Admin") ? "Admin" : "User";
                userRoles[user.Id] = userRole;

                _logger.LogInformation("User {UserId} has roles: {Roles}", user.Id, string.Join(", ", roles));
            }

            ViewData["UserRoles"] = userRoles;
            return View(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ManageUsers.");
            return StatusCode(500, "An unexpected error occurred while loading users.");
        }
    }


    [Authorize(Roles = "Admin, SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> AssignAdmin(string userId)
    {
        try
        {
            _logger.LogInformation("AssignAdmin called for userId: {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("AssignAdmin failed: userId is null or empty.");
                TempData["Error"] = "Invalid user selection.";
                return RedirectToAction(nameof(ManageUsers));
            }


            var success = await _roleService.AssignUserToAdminAsync(userId);

            if (success)
            {
                TempData["Success"] = "User has been assigned as Admin.";
                TempData["RoleChangedUserId"] = userId;
                _logger.LogInformation("Successfully assigned Admin role to user {UserId}", userId);
            }
            else
            {
                TempData["Error"] = "Failed to assign Admin role.";
                _logger.LogWarning("Failed to assign Admin role to user {UserId}", userId);
            }

            return RedirectToAction(nameof(ManageUsers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AssignAdmin for user {UserId}", userId);
            TempData["Error"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(ManageUsers));
        }
    }


    [Authorize(Roles = "Admin, SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> RemoveAdmin(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Invalid user selection.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var success = await _roleService.RemoveAdminRoleAsync(userId);

            TempData[success ? "Success" : "Error"] = success ? "Admin role has been removed." : "Failed to remove Admin role.";

            if (success) TempData["RoleChangedUserId"] = userId;

            return RedirectToAction(nameof(ManageUsers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing Admin role from user {UserId}.", userId);
            TempData["Error"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(ManageUsers));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Invalid user selection.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var member = await _memberService.GetMemberByUserIdAsync(userId);
            if (member != null)
            {
                await _memberService.DeleteMemberAsync(member.Id);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User and related member deleted.";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}.", userId);
            TempData["Error"] = "An unexpected error occurred while deleting the user.";
            return RedirectToAction(nameof(ManageUsers));
        }
    }

}