// Not Ready and not included in the project

public static class CookieHelpers
{
    public static void ClearCookies(HttpResponse response)
    {
        response.Cookies.Delete(".AspNetCore.Identity.Application");   

        response.Cookies.Delete("__RequestVerificationToken");

        response.Cookies.Delete("_ga");
        response.Cookies.Delete("_gid");
        response.Cookies.Delete("_gat");
        response.Cookies.Delete("fbp");

        response.Cookies.Delete("my-shopping-cart");
        response.Cookies.Delete("CookieConsent");

        response.Headers.Append(
            "Clear-Site-Data",
            "\"cookies\", \"storage\"");
    }
}
