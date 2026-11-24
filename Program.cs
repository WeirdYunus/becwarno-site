using System.Globalization;
using Becwarno.Web.Data;
using Becwarno.Web.Models;
using Becwarno.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ---- Services ---------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection("Admin"));
builder.Services.AddSingleton<ImageStorage>();

// SQLite database. A relative path is resolved against the app folder.
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=App_Data/becwarno.db";
var csb = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(csb.DataSource))
{
    csb.DataSource = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
}
Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(csb.ToString()));

// Cookie authentication for the admin area.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "becwarno.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// Hosting platforms terminate HTTPS in front of the app.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// ---- Database ---------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    SeedData.Initialize(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

// ---- Pipeline ---------------------------------------------------------------
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Always parse and print numbers like 12.50 the same way, whatever the server's language is.
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture }
});

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data: https:; style-src 'self'; script-src 'self'; " +
        "font-src 'self'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    await next();
});

app.UseStaticFiles();

// Uploaded images live outside wwwroot so they survive redeploys when pointed at a persistent folder.
var storage = app.Services.GetRequiredService<ImageStorage>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.RootPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
