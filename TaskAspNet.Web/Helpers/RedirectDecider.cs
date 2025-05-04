using Microsoft.AspNetCore.Mvc;

namespace TaskAspNet.Web.Helpers
{
    public static class RedirectDecider
    {
        public static string Decide(IList<string> roles, bool hasProfile, IUrlHelper url, string? returnUrl)
        {
            if (roles.Contains("SuperAdmin"))
                return url.Action("Index", "Project")!;

            if (roles.Contains("Admin"))
                return url.Action("Index", "Project")!;

            if (!hasProfile)
                return url.Action("CompleteProfile", "Member")!;

            if (!string.IsNullOrEmpty(returnUrl) && url.IsLocalUrl(returnUrl))
                return returnUrl;

            return url.Action("Index", "Project")!;
        }
    }
}
