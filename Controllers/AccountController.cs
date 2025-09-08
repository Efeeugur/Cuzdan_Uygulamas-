using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cüzdan_Uygulaması.Models;
using Cüzdan_Uygulaması.Models.ViewModels;
using Cüzdan_Uygulaması.Logging;

namespace Cüzdan_Uygulaması.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        if (!ModelState.IsValid)
        {
            _logger.LogValidationError("Anonymous", "Registration", "Model validation failed");
            return View(model);
        }

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogUserRegistration(user.Id, user.UserName, ipAddress);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogUserLogin(user.Id, user.UserName, ipAddress);
            return RedirectToAction("Index", "Home");
        }

        var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("User registration failed for {Email} from {IpAddress}. Errors: {Errors}",
            model.Email, ipAddress, errorMessages);

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        if (!ModelState.IsValid)
        {
            _logger.LogValidationError("Anonymous", "Login", "Model validation failed");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                _logger.LogUserLogin(user.Id, user.UserName, ipAddress);
            }
            
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        string reason = result.IsLockedOut ? "Account locked" : 
                       result.IsNotAllowed ? "Account not allowed" :
                       result.RequiresTwoFactor ? "Two factor required" :
                       "Invalid credentials";
        
        _logger.LogLoginFailed(model.Email, ipAddress, reason);
        ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var userId = User?.Identity?.Name;
        var username = User?.Identity?.Name;
        
        await _signInManager.SignOutAsync();
        
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
        {
            _logger.LogUserLogout(userId, username);
        }
        
        return RedirectToAction("Index", "Home");
    }
}