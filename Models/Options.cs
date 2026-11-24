namespace Becwarno.Web.Models;

public class SiteOptions
{
    public string Name { get; set; } = "Becwarno";
    public string Tagline { get; set; } = "";
    public string EtsyUrl { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string CurrencySymbol { get; set; } = "£";
}

public class AdminOptions
{
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "";
}
