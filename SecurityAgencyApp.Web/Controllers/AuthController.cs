using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Authentication.Commands.Login;
using SecurityAgencyApp.Application.Features.Authentication.Commands.RegisterAgency;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

public class AuthController : Controller
{
    private readonly IMediator _mediator;
    private readonly IApiClient _apiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, IApiClient apiClient, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterAgencyCommand command)
    {
        if (!ModelState.IsValid)
        {
            return View(command);
        }

        var result = await _mediator.Send(command);

        if (result.Success && result.Data != null)
        {
            TempData["SuccessMessage"] = result.Data.Message;
            return RedirectToAction("Login", new { message = "Registration successful. Please login with your credentials." });
        }

        ModelState.AddModelError("", result.Message ?? "Registration failed");
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginCommand command, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(command);
        }

        // Call API for login (Web lightweight – no MediatR for auth)
        var result = await _apiClient.PostAsync<LoginResponse>("api/v1/Auth/login", new
        {
            userName = command.UserName,
            password = command.Password,
            rememberMe = command.RememberMe
        });

        if (result.Success && result.Data != null && result.Data.User != null)
        {
            var d = result.Data;
            var user = d.User;
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("TenantId", user.TenantId.ToString());
            HttpContext.Session.SetString("TenantName", user.TenantName);
            HttpContext.Session.SetString("AccessToken", d.AccessToken);
            HttpContext.Session.SetString("Roles", user.Roles != null ? string.Join(",", user.Roles) : "");
            HttpContext.Session.SetString("IsSupervisor", (user.IsSupervisor == true).ToString());

            if (command.RememberMe)
            {
                var options = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("RememberMe", "true", options);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", result.Message ?? "Invalid username or password");
        return View(command);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Please enter your email");
            return View();
        }

        var result = await _apiClient.PostAsync<Models.Api.ForgotPasswordResponse>("api/v1/Auth/forgot-password", new { email = email.Trim() });
        if (result.Success && result.Data != null)
        {
            TempData["ForgotPasswordEmail"] = result.Data.Email;
            TempData["ForgotPasswordUserName"] = result.Data.UserName;
            TempData["ForgotPasswordPassword"] = result.Data.Password;
            return RedirectToAction(nameof(ForgotPasswordResult));
        }

        ModelState.AddModelError("", result.Message ?? "No account found with this email");
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPasswordResult()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email = null)
    {
        ViewData["Email"] = email ?? "";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Email is required.");
            return View();
        }
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            ModelState.AddModelError("", "New password must be at least 6 characters.");
            return View();
        }
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "New password and confirm password do not match.");
            return View();
        }

        var result = await _apiClient.PostAsync<object>("api/v1/Auth/reset-password", new { email = email.Trim(), newPassword });
        if (result.Success)
        {
            TempData["ResetPasswordMessage"] = result.Message ?? "Password updated. You can now login.";
            return RedirectToAction("Login", new { message = TempData["ResetPasswordMessage"] });
        }

        ModelState.AddModelError("", result.Message ?? "Failed to reset password.");
        ViewData["Email"] = email;
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete("RememberMe");
        return RedirectToAction("Login", "Auth");
    }
}
