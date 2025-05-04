namespace TaskAspNet.Business.ViewModel;

public class CookieConsentViewModel
{
    public bool IsConsentGiven { get; set; }
    public bool NecessaryCookies { get; set; } = true; 
    public bool FunctionalCookies { get; set; }
    public bool AnalyticsCookies { get; set; }
    public bool MarketingCookies { get; set; }
    public bool AdvertisingCookies { get; set; }
}
