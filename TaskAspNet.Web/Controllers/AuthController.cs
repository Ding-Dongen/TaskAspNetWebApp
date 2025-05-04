using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskAspNet.Data.EntityIdentity;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Web.Interfaces;
using TaskAspNet.Business.ViewModel;
using TaskAspNet.Web.Helpers;

namespace TaskAspNet.Web.Controllers
{
    public class AuthController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IMemberService memberService,
        IWebHostEnvironment webHostEnvironment,
        IGoogleAuthHandler googleAuthHandler, 
        ILogger<AuthController> logger) : Controller
    {
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IMemberService _memberService = memberService;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IGoogleAuthHandler _googleAuthHandler = googleAuthHandler;
        private readonly ILogger<AuthController> _log = logger;


        public IActionResult CreateAcc()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error displaying CreateAcc view.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAcc(UserRegistrationForm form)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(form);

                var existingUser = await _userManager.FindByEmailAsync(form.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "A user with that email already exists.");
                    return View(form);
                }

                var nameParts = form.FullName?.Trim().Split(' ', 2);
                var firstName = nameParts?.Length > 0 ? nameParts[0] : "Unknown";
                var lastName = nameParts?.Length > 1 ? nameParts[1] : "";

                var user = new AppUser
                {
                    UserName = form.Email,
                    Email = form.Email,
                    FirstName = firstName,
                    LastName = lastName
                };

                var createResult = await _userManager.CreateAsync(user, form.Password);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(form);
                }

                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction(
                    "CompleteProfile",
                    "Member",
                    new { fullName = form.FullName, email = form.Email, userId = user.Id });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating user account for {Email}.", form.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(form);
            }
        }

        public IActionResult LogIn(string? returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error displaying LogIn view.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(UserLogInForm form, string? returnUrl = null)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(form.Email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, form.Password))
                {
                    ModelState.AddModelError("", "Invalid login. Please check your email and password.");
                    return View(form);
                }

                var signInResult = await _signInManager.PasswordSignInAsync(
                    form.Email, form.Password, form.RememberMe, lockoutOnFailure: false);

                if (!signInResult.Succeeded)
                {
                    ModelState.AddModelError("", "Login failed. Please try again.");
                    return View(form);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var hasProfile = (await _memberService.GetAllMembersAsync())
                                 .Any(m => m.Email == user.Email);

                await _signInManager.SignInAsync(user, form.RememberMe);

                var targetUrl = RedirectDecider.Decide(roles, hasProfile, Url, returnUrl);
                return Redirect(targetUrl);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error during login for {Email}.", form.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(form);
            }
        }

        public async Task<IActionResult> LogOut()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return View("ExternalLogoutRedirect", Url.Action("LogIn", "Auth"));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error during logout.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            try
            {
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error initiating external login with {Provider}.", provider);
                TempData["Error"] = "An unexpected error occurred while starting external login.";
                return RedirectToAction("LogIn");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(remoteError))
                {
                    TempData["Error"] = $"External login failed: {remoteError}";
                    return RedirectToAction("LogIn");
                }

                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    TempData["Error"] = "Could not load external login info.";
                    return RedirectToAction("LogIn");
                }

                return await _googleAuthHandler.HandleExternalLoginAsync(info);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error handling external login callback.");
                TempData["Error"] = "An unexpected error occurred during external login.";
                return RedirectToAction("LogIn");
            }
        }

    }
}
