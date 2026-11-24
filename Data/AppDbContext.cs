using Becwarno.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Becwarno.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PortfolioItem> Items => Set<PortfolioItem>();
    public DbSet<ContactMessage> Messages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioItem>(e =>
        {
            e.Property(p => p.Title).IsRequired().HasMaxLength(120);
            e.Property(p => p.Price).HasPrecision(10, 2);
            e.HasIndex(p => p.Type);
        });

        modelBuilder.Entity<ContactMessage>(e =>
        {
            e.Property(m => m.Name).IsRequired().HasMaxLength(80);
            e.Property(m => m.Email).IsRequired().HasMaxLength(120);
            e.Property(m => m.Message).IsRequired().HasMaxLength(2000);
        });
    }
}
