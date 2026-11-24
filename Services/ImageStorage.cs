namespace Becwarno.Web.Services;

/// <summary>Validates and stores uploaded images on disk. Files are served from /uploads.</summary>
public class ImageStorage
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxBytes = 5 * 1024 * 1024;

    public string RootPath { get; }

    public static bool IsSupportedImage(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant());

    public ImageStorage(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["Storage:UploadsPath"];
        if (string.IsNullOrWhiteSpace(configured)) configured = "App_Data/uploads";

        RootPath = Path.IsPathRooted(configured) ? configured : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(RootPath);
    }

    public async Task<(string? Url, string? Error)> SaveAsync(IFormFile file)
    {
        if (file.Length == 0) return (null, "The selected file is empty.");
        if (file.Length > MaxBytes) return (null, "The image must be 5 MB or smaller.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!IsSupportedImage(file.FileName)) return (null, "Only JPG, PNG, WEBP or GIF images are allowed.");

        var header = new byte[12];
        int read;
        await using (var probe = file.OpenReadStream())
        {
            read = await probe.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        }
        if (read < 12 || !LooksLikeImage(header)) return (null, "That file does not look like a valid image.");

        var name = Guid.NewGuid().ToString("N") + ext;
        await using (var target = File.Create(Path.Combine(RootPath, name)))
        {
            await file.CopyToAsync(target);
        }
        return ("/uploads/" + name, null);
    }

    /// <summary>Deletes a previously uploaded file. External URLs are ignored.</summary>
    public void Delete(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/", StringComparison.Ordinal)) return;

        var path = Path.Combine(RootPath, Path.GetFileName(url));
        if (File.Exists(path)) File.Delete(path);
    }

    private static bool LooksLikeImage(byte[] h)
    {
        bool jpg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        bool png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
        bool gif = h[0] == 'G' && h[1] == 'I' && h[2] == 'F' && h[3] == '8';
        bool webp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                    && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
        return jpg || png || gif || webp;
    }
}
