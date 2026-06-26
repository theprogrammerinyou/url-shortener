using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure;

public sealed class UrlShortenerDbContext : DbContext
{
    public DbSet<UrlEntry> UrlMappings { get; set; } = null!;
    public DbSet<ClickEvent> ClickEvents { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<AppUser> AppUsers { get; set; } = null!;

    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UrlEntry>(entity =>
        {
            entity.ToTable("UrlMappings");
            entity.HasKey(x => x.ShortCode);
            entity.Property(x => x.ShortCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OriginalUrl).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(100);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.Property(x => x.ClickCount).IsRequired();
            entity.Property(x => x.QrScanCount).IsRequired();
            entity.Property(x => x.IsCustom).IsRequired();
            entity.Property(x => x.IsPrivate).IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.OriginalUrl);
        });

        modelBuilder.Entity<ClickEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ShortCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ClickedAt).IsRequired();
            entity.HasIndex(x => x.ShortCode);
            entity.HasIndex(x => x.ClickedAt);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DefaultDomain).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ApiKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Theme).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Plan).HasMaxLength(50).IsRequired().HasDefaultValue("free");
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}
