using TaskAspNet.Business.Helper;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Data.Entities;
using TaskAspNet.Data.Interfaces;
using TaskAspNet.Services.Interfaces;

namespace TaskAspNet.Services;

public class ConsentService : IConsentService
{
    private readonly IConsentRepository _consentRepository;
    private readonly IUnitOfWork _uow;

    public ConsentService(IConsentRepository consentRepository, IUnitOfWork uow)
    {
        _consentRepository = consentRepository;
        _uow = uow;
    }

    public async Task<Consent?> GetConsentByUserIdAsync(string userId)
    {
        return await _consentRepository.GetByUserIdAsync(userId);
    }

    public async Task SaveUserConsentAsync(string userId, CookieConsentViewModel model)
    {
        await _uow.ExecuteAsync(async () =>
        {
            var existingConsent = await _consentRepository.GetByUserIdAsync(userId);

            if (existingConsent is null)
            {
                await _consentRepository.AddAsync(new Consent
                {
                    UserId = userId,
                    IsConsentGiven = model.FunctionalCookies || model.AnalyticsCookies ||
                                         model.MarketingCookies || model.AdvertisingCookies,
                    FunctionalCookies = model.FunctionalCookies,
                    AnalyticsCookies = model.AnalyticsCookies,
                    MarketingCookies = model.MarketingCookies,
                    AdvertisingCookies = model.AdvertisingCookies,
                    DateGiven = DateTime.UtcNow
                });
            }
            else
            {
                existingConsent.IsConsentGiven = model.FunctionalCookies || model.AnalyticsCookies ||
                                                     model.MarketingCookies || model.AdvertisingCookies;
                existingConsent.FunctionalCookies = model.FunctionalCookies;
                existingConsent.AnalyticsCookies = model.AnalyticsCookies;
                existingConsent.MarketingCookies = model.MarketingCookies;
                existingConsent.AdvertisingCookies = model.AdvertisingCookies;
                existingConsent.DateGiven = DateTime.UtcNow;

                await _consentRepository.UpdateAsync(existingConsent);
            }
        });
        }   

    }
