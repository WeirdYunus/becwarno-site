using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Becwarno.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Becwarno.Web.Controllers;

public class AccountController : Controller
{
    private readonly AdminOptions _admin;

    public AccountController(IOptions<AdminOptions> admin) => _admin = admin.Value;

    [HttpGet("/admin/login")]
    public IActionResult Login(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrEmpty(_admin.Password))
        {
            ModelState.AddModelError("", "Admin login is disabled because no admin password is configured on the server.");
            return View(model);
        }

        if (!ModelState.IsValid) return View(model);

        bool userOk = SafeEquals(model.Username, _admin.Username);
        bool passOk = SafeEquals(model.Password, _admin.Password);

        if (!(userOk && passOk))
        {
            await Task.Delay(500); // slow down guessing
            ModelState.AddModelError("", "Wrong username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _admin.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return Redirect("/admin");
    }

    [HttpPost("/admin/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private static bool SafeEquals(string a, string b)
    {
        var ha = SHA256.HashData(Encoding.UTF8.GetBytes(a ?? ""));
        var hb = SHA256.HashData(Encoding.UTF8.GetBytes(b ?? ""));
        return CryptographicOperations.FixedTimeEquals(ha, hb);
    }
}
