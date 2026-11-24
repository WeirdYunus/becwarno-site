using Becwarno.Web.Data;
using Becwarno.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Becwarno.Web.Controllers;

public class GalleryController : Controller
{
    private readonly AppDbContext _db;

    public GalleryController(AppDbContext db) => _db = db;

    [HttpGet("/shop")]
    public Task<IActionResult> Shop(string? category, string? q) => Build(ItemType.Product, category, q);

    [HttpGet("/paintings")]
    public Task<IActionResult> Paintings(string? category) => Build(ItemType.Painting, category, null);

    [HttpGet("/tattoos")]
    public Task<IActionResult> Tattoos(string? category) => Build(ItemType.Tattoo, category, null);

    private async Task<IActionResult> Build(ItemType type, string? category, string? q)
    {
        category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        if (q != null && q.Length > 60) q = q.Substring(0, 60);

        var query = _db.Items.Where(i => i.Type == type);

        if (category != null) query = query.Where(i => i.Category == category);
        if (q != null)
        {
            query = query.Where(i => i.Title.Contains(q) || (i.Description != null && i.Description.Contains(q)));
        }

        var items = await query.OrderByDescending(i => i.Id).ToListAsync();

        var rawCategories = await _db.Items
            .Where(i => i.Type == type && i.Category != null)
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync();

        var categories = rawCategories
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .OrderBy(c => c)
            .ToList();

        var model = new GalleryViewModel
        {
            Type = type,
            Items = items,
            Categories = categories,
            SelectedCategory = category,
            Query = q
        };

        var viewName = type switch
        {
            ItemType.Product => "Shop",
            ItemType.Painting => "Paintings",
            _ => "Tattoos"
        };
        return View(viewName, model);
    }
}
