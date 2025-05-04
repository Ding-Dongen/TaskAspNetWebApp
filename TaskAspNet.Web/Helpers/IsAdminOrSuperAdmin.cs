using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAdminOrSuperAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("Admin") || user.IsInRole("SuperAdmin");
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("SuperAdmin");
    }
}
