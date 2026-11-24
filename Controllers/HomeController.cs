using Becwarno.Web.Data;
using Becwarno.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Becwarno.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var featured = await _db.Items
            .Where(i => i.IsFeatured)
            .OrderByDescending(i => i.Id)
            .Take(8)
            .ToListAsync();
        return View(featured);
    }

    [HttpGet("/about")]
    public IActionResult About() => View(new ContactForm());

    [HttpPost("/about")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> About(ContactForm form)
    {
        // Bots tend to fill every field. Pretend it worked, store nothing.
        if (!string.IsNullOrWhiteSpace(form.Website))
        {
            TempData["Sent"] = true;
            return Redirect("/about#contact");
        }

        if (!ModelState.IsValid) return View(form);

        _db.Messages.Add(new ContactMessage
        {
            Name = form.Name.Trim(),
            Email = form.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(form.Subject) ? null : form.Subject.Trim(),
            Message = form.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Sent"] = true;
        return Redirect("/about#contact");
    }

    [HttpGet("/error")]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View();
    }
}
