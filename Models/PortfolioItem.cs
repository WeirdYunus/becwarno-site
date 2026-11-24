using System.ComponentModel.DataAnnotations;

namespace Becwarno.Web.Models;

public enum ItemType
{
    Product = 0,   // Etsy shop item
    Painting = 1,  // canvas / artwork
    Tattoo = 2     // tattoo studio piece
}

public class PortfolioItem : IValidatableObject
{
    public int Id { get; set; }

    public ItemType Type { get; set; }

    [StringLength(120, ErrorMessage = "The title cannot be longer than 120 characters.")]
    public string Title { get; set; } = "";

    [StringLength(1000, ErrorMessage = "The description can be at most 1000 characters.")]
    public string? Description { get; set; }

    /// <summary>Product category, painting medium or tattoo style depending on <see cref="Type"/>.</summary>
    [StringLength(60, ErrorMessage = "The category can be at most 60 characters.")]
    public string? Category { get; set; }

    [Range(0, 100000, ErrorMessage = "The price must be between 0 and 100000.")]
    public decimal? Price { get; set; }

    [StringLength(500)]
    [RegularExpression(@"^(https?://|/uploads/).+", ErrorMessage = "The image must be an http(s) link or an uploaded file.")]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    [RegularExpression(@"^https?://.+", ErrorMessage = "The link must start with http:// or https://")]
    public string? ExternalUrl { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type != ItemType.Tattoo && string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Please enter a title.", new[] { nameof(Title) });
        }
        else if (!string.IsNullOrWhiteSpace(Title) && Title.Length < 2)
        {
            yield return new ValidationResult("The title must be at least 2 characters.", new[] { nameof(Title) });
        }
    }
}
