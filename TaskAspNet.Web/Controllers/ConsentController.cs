using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Services.Interfaces;

public class ConsentController(IConsentService consentService, ILogger<ConsentController> logger) : Controller
{
    private readonly IConsentService _consentService = consentService;
    private readonly ILogger<ConsentController> _logger = logger;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User?.Identity?.Name ?? "anonymous";
            var existingConsent = await _consentService.GetConsentByUserIdAsync(userId);

            CookieConsentViewModel vm;

            if (existingConsent == null)
            {
                vm = new CookieConsentViewModel
                {
                    NecessaryCookies = true,
                    IsConsentGiven = false,
                    FunctionalCookies = false,
                    AnalyticsCookies = false,
                    MarketingCookies = false,
                    AdvertisingCookies = false
                };
                ViewBag.HasConsent = false;
            }
            else
            {
                vm = new CookieConsentViewModel
                {
                    NecessaryCookies = true,
                    IsConsentGiven = existingConsent.IsConsentGiven,
                    FunctionalCookies = existingConsent.FunctionalCookies,
                    AnalyticsCookies = existingConsent.AnalyticsCookies,
                    MarketingCookies = existingConsent.MarketingCookies,
                    AdvertisingCookies = existingConsent.AdvertisingCookies
                };
                ViewBag.HasConsent = existingConsent.IsConsentGiven;
            }

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cookie‑consent page.");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] CookieConsentViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var userId = User?.Identity?.Name ?? "anonymous";

            model.NecessaryCookies = true;
            model.IsConsentGiven = model.FunctionalCookies || model.AnalyticsCookies
                                   || model.MarketingCookies || model.AdvertisingCookies;

            await _consentService.SaveUserConsentAsync(userId, model);

            Response.Cookies.Append("CookieConsent", "true", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cookie consent for user {UserId}.", User?.Identity?.Name ?? "anonymous");
            return Json(new
            {
                success = false,
                message = "An unexpected error occurred while saving cookie consent."
            });
        }
    }
}
