using Becwarno.Web.Data;
using Becwarno.Web.Models;
using Becwarno.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Becwarno.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    private const string BindFields = "Type,Title,Description,Category,Price,ImageUrl,ExternalUrl,IsFeatured";

    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public AdminController(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    // ---- Dashboard -----------------------------------------------------------
    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            Products = await _db.Items.CountAsync(i => i.Type == ItemType.Product),
            Paintings = await _db.Items.CountAsync(i => i.Type == ItemType.Painting),
            Tattoos = await _db.Items.CountAsync(i => i.Type == ItemType.Tattoo),
            UnreadMessages = await _db.Messages.CountAsync(m => !m.IsRead)
        };
        ViewData["Recent"] = await _db.Items.OrderByDescending(i => i.Id).Take(5).ToListAsync();
        return View(model);
    }

    // ---- Items: list (Read) --------------------------------------------------
    [HttpGet("/admin/items")]
    public async Task<IActionResult> Items(ItemType type = ItemType.Product)
    {
        ViewData["Type"] = type;
        var items = await _db.Items.Where(i => i.Type == type).OrderByDescending(i => i.Id).ToListAsync();
        return View(items);
    }

    // ---- Items: create -------------------------------------------------------
    [HttpGet("/admin/items/create")]
    public IActionResult Create(ItemType type = ItemType.Product)
    {
        return View("Form", new PortfolioItem { Type = type });
    }

    [HttpPost("/admin/items/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(BindFields)] PortfolioItem item, IFormFile? imageFile)
    {
        string? uploaded = null;
        if (imageFile != null && imageFile.Length > 0)
        {
            var result = await _images.SaveAsync(imageFile);
            if (result.Error != null) ModelState.AddModelError(nameof(PortfolioItem.ImageUrl), result.Error);
            else uploaded = result.Url;
        }

        if (!ModelState.IsValid)
        {
            _images.Delete(uploaded);
            return View("Form", item);
        }

        if (uploaded != null) item.ImageUrl = uploaded;
        item.CreatedAt = DateTime.UtcNow;

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        TempData["Flash"] = "Item created.";
        return RedirectToAction(nameof(Items), new { type = item.Type });
    }

    // ---- Tattoos: bulk create -----------------------------------------------
    [HttpPost("/admin/items/tattoos/bulk")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(115343360)]
    public async Task<IActionResult> BulkCreateTattoos(List<IFormFile>? imageFiles)
    {
        var selectedFiles = imageFiles ?? new List<IFormFile>();
        var files = selectedFiles
            .Where(file => file.Length > 0 && ImageStorage.IsSupportedImage(file.FileName))
            .ToList();
        var skippedCount = selectedFiles.Count - files.Count;

        if (files.Count == 0)
        {
            TempData["FlashError"] = "Select at least one JPG, PNG, WEBP or GIF image.";
            return RedirectToAction(nameof(Items), new { type = ItemType.Tattoo });
        }

        if (files.Count > 100)
        {
            TempData["FlashError"] = "You can upload at most 100 tattoo images at a time.";
            return RedirectToAction(nameof(Items), new { type = ItemType.Tattoo });
        }

        var uploadedUrls = new List<string>();
        var tattoos = new List<PortfolioItem>();

        try
        {
            foreach (var file in files)
            {
                var result = await _images.SaveAsync(file);
                if (result.Error != null)
                {
                    foreach (var url in uploadedUrls) _images.Delete(url);
                    TempData["FlashError"] = $"Could not add '{file.FileName}': {result.Error} No tattoos were added.";
                    return RedirectToAction(nameof(Items), new { type = ItemType.Tattoo });
                }

                var imageUrl = result.Url!;
                uploadedUrls.Add(imageUrl);

                tattoos.Add(new PortfolioItem
                {
                    Type = ItemType.Tattoo,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _db.Items.AddRange(tattoos);
            await _db.SaveChangesAsync();
        }
        catch
        {
            foreach (var url in uploadedUrls) _images.Delete(url);
            throw;
        }

        TempData["Flash"] = $"Added {tattoos.Count} tattoos." +
            (skippedCount > 0 ? $" Skipped {skippedCount} unsupported or empty files." : "");
        return RedirectToAction(nameof(Items), new { type = ItemType.Tattoo });
    }

    // ---- Items: edit ---------------------------------------------------------
    [HttpGet("/admin/items/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();
        return View("Form", item);
    }

    [HttpPost("/admin/items/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(BindFields)] PortfolioItem form, IFormFile? imageFile)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();

        string? uploaded = null;
        if (imageFile != null && imageFile.Length > 0)
        {
            var result = await _images.SaveAsync(imageFile);
            if (result.Error != null) ModelState.AddModelError(nameof(PortfolioItem.ImageUrl), result.Error);
            else uploaded = result.Url;
        }

        if (!ModelState.IsValid)
        {
            _images.Delete(uploaded);
            form.Id = id;
            form.CreatedAt = item.CreatedAt;
            return View("Form", form);
        }

        var oldImage = item.ImageUrl;

        item.Type = form.Type;
        item.Title = form.Title;
        item.Description = form.Description;
        item.Category = form.Category;
        item.Price = form.Type == ItemType.Product ? form.Price : null;
        item.ExternalUrl = form.Type == ItemType.Product || form.Type == ItemType.Painting ? form.ExternalUrl : null;
        item.IsFeatured = form.IsFeatured;
        item.ImageUrl = uploaded ?? form.ImageUrl;

        await _db.SaveChangesAsync();

        // Remove the old uploaded file when it is no longer used.
        if (oldImage != null && oldImage != item.ImageUrl) _images.Delete(oldImage);

        TempData["Flash"] = "Changes saved.";
        return RedirectToAction(nameof(Items), new { type = item.Type });
    }

    // ---- Items: delete -------------------------------------------------------
    [HttpPost("/admin/items/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return NotFound();

        var type = item.Type;
        var image = item.ImageUrl;

        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
        _images.Delete(image);

        TempData["Flash"] = "Item deleted.";
        return RedirectToAction(nameof(Items), new { type });
    }

    // ---- Messages ------------------------------------------------------------
    [HttpGet("/admin/messages")]
    public async Task<IActionResult> Messages()
    {
        var messages = await _db.Messages.OrderByDescending(m => m.Id).ToListAsync();
        return View(messages);
    }

    [HttpPost("/admin/messages/{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRead(int id)
    {
        var message = await _db.Messages.FindAsync(id);
        if (message == null) return NotFound();

        message.IsRead = !message.IsRead;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Messages));
    }

    [HttpPost("/admin/messages/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = await _db.Messages.FindAsync(id);
        if (message == null) return NotFound();

        _db.Messages.Remove(message);
        await _db.SaveChangesAsync();

        TempData["Flash"] = "Message deleted.";
        return RedirectToAction(nameof(Messages));
    }
}
