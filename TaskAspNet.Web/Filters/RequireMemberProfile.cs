using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Data.EntityIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

// Created with the help of ChatGPT4.5
// Apply this attribute to controllers or actions to enforce a “member profile” exists
// Checks if another filter has set a result 
// Retrieve the claimas principle  and check if not authenticated let other policies do the work
// Check if the endpoint has AllowAnonymous attribute
// Check if the user is in the SuperAdmin role
// Check if there is an existing profile
// Gets all the members and checks if the email exists
// If the user does not have a profile, redirect to the CompleteProfile action or let them though

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireMemberProfile : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext ctx)
    {
        if (ctx.Result != null) return;

        var user = ctx.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true) return;

        var endpoint = ctx.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null) return;

        var userMgr = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
        var appUser = await userMgr.GetUserAsync(user);
        if (appUser == null) return;

        if (await userMgr.IsInRoleAsync(appUser, "SuperAdmin"))
            return;

        var memberSvc = ctx.HttpContext.RequestServices.GetRequiredService<IMemberService>();

        bool hasProfile = await memberSvc
            .GetAllMembersAsync()
            .ContinueWith(t => t.Result.Any(m => m.Email == appUser.Email));

        if (hasProfile) return;

        ctx.Result = new RedirectToActionResult(
            actionName: "CompleteProfile",
            controllerName: "Member",
            routeValues: new
            {
                fullName = $"{appUser.FirstName} {appUser.LastName}".Trim(),
                email = appUser.Email,
                userId = appUser.Id
            });
    }
}

