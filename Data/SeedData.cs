using Becwarno.Web.Models;

namespace Becwarno.Web.Data;

public static class SeedData
{
    /// <summary>Creates the database if needed and adds a few sample entries on first run.</summary>
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Items.Any()) return;

        var now = DateTime.UtcNow;

        db.Items.AddRange(
            new PortfolioItem { Type = ItemType.Product, Title = "Raven Feather Pendant", Category = "Jewelry", Price = 24.00m, IsFeatured = true,
                Description = "Hand-finished pendant with a tiny raven feather, strung on blackened chain.", CreatedAt = now.AddMinutes(-9) },
            new PortfolioItem { Type = ItemType.Product, Title = "Cathedral Candle Holder", Category = "Home decor", Price = 32.00m, IsFeatured = true,
                Description = "Pointed-arch candle holder for the long, dim evenings.", CreatedAt = now.AddMinutes(-8) },
            new PortfolioItem { Type = ItemType.Product, Title = "Moth Sticker Pack", Category = "Stickers", Price = 6.50m,
                Description = "Five matte vinyl stickers of nocturnal things.", CreatedAt = now.AddMinutes(-7) },

            new PortfolioItem { Type = ItemType.Painting, Title = "Nocturne in Crimson", Category = "Acrylic", IsFeatured = true,
                Description = "Acrylic on canvas.", CreatedAt = now.AddMinutes(-6) },
            new PortfolioItem { Type = ItemType.Painting, Title = "The Hollow Cathedral", Category = "Oil",
                Description = "Oil on canvas.", CreatedAt = now.AddMinutes(-5) },
            new PortfolioItem { Type = ItemType.Painting, Title = "Moth Saint", Category = "Mixed media",
                Description = "Mixed media on panel.", CreatedAt = now.AddMinutes(-4) },

            new PortfolioItem { Type = ItemType.Tattoo, Title = "Dagger & Roses", Category = "Traditional", IsFeatured = true,
                Description = "Bold lines, deep reds.", CreatedAt = now.AddMinutes(-3) },
            new PortfolioItem { Type = ItemType.Tattoo, Title = "Moth Memento", Category = "Blackwork",
                Description = "Solid black with fine stippling.", CreatedAt = now.AddMinutes(-2) },
            new PortfolioItem { Type = ItemType.Tattoo, Title = "Spine Cathedral", Category = "Fine line",
                Description = "Delicate architecture along the spine.", CreatedAt = now.AddMinutes(-1) }
        );

        db.SaveChanges();
    }
}
