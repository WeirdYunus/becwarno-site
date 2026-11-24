using System.ComponentModel.DataAnnotations;

namespace Becwarno.Web.Models;

public class ContactForm
{
    [Required(ErrorMessage = "Please tell us your name.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Your name must be between 2 and 80 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
    [StringLength(120)]
    public string Email { get; set; } = "";

    [StringLength(120, ErrorMessage = "The subject can be at most 120 characters.")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "Please write a message.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Your message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = "";

    /// <summary>Honeypot field: real visitors never see or fill it in.</summary>
    public string? Website { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class GalleryViewModel
{
    public ItemType Type { get; set; }
    public List<PortfolioItem> Items { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public string? Query { get; set; }
}

public class DashboardViewModel
{
    public int Products { get; set; }
    public int Paintings { get; set; }
    public int Tattoos { get; set; }
    public int UnreadMessages { get; set; }
}
