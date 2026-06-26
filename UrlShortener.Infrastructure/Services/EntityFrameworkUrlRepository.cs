using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class EntityFrameworkUrlRepository : IUrlRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public EntityFrameworkUrlRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(UrlEntry urlEntry)
    {
        if (urlEntry == null) throw new ArgumentNullException(nameof(urlEntry));

        await _dbContext.UrlMappings.AddAsync(urlEntry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UrlEntry?> FindByShortCodeAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode)) throw new ArgumentException("Short code is required.", nameof(shortCode));

        return await _dbContext.UrlMappings.FindAsync(shortCode);
    }

    public async Task<UrlEntry?> FindByOriginalUrlAsync(string originalUrl)
    {
        if (string.IsNullOrWhiteSpace(originalUrl)) throw new ArgumentException("Original URL is required.", nameof(originalUrl));

        return await _dbContext.UrlMappings.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
    }

    public async Task<IEnumerable<UrlEntry>> GetAllAsync(string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Enumerable.Empty<UrlEntry>();
        }

        var query = _dbContext.UrlMappings.AsQueryable();
        query = query.Where(x => x.UserId == userId);

        return await query.OrderByDescending(x => x.CreatedAt).ToArrayAsync();
    }

    public async Task<(IEnumerable<UrlEntry> Items, int TotalCount)> GetPagedAsync(
        string? userId,
        string? search,
        string? status,
        int page,
        int pageSize)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return (Enumerable.Empty<UrlEntry>(), 0);
        }

        var query = _dbContext.UrlMappings.AsQueryable();
        var now = DateTime.UtcNow;

        query = query.Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ShortCode.ToLower().Contains(term) ||
                x.OriginalUrl.ToLower().Contains(term));
        }

        status = status?.Trim().ToLowerInvariant();
        query = status switch
        {
            "active" => query.Where(x => x.ExpiresAt >= now),
            "expired" => query.Where(x => x.ExpiresAt < now),
            "private" => query.Where(x => x.IsPrivate),
            _ => query
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize))
            .Take(pageSize)
            .ToArrayAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<UrlEntry>> GetRecentAsync(string? userId, int limit)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<UrlEntry>();
        }

        var query = _dbContext.UrlMappings.AsQueryable();
        query = query.Where(x => x.UserId == userId);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task DeleteAsync(string shortCode, string? userId)
    {
        var urlEntry = await FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(userId) && urlEntry.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this link.");
        }

        _dbContext.UrlMappings.Remove(urlEntry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(UrlEntry urlEntry)
    {
        if (urlEntry == null) throw new ArgumentNullException(nameof(urlEntry));

        _dbContext.UrlMappings.Update(urlEntry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode)) throw new ArgumentException("Short code is required.", nameof(shortCode));

        return await _dbContext.UrlMappings.AnyAsync(x => x.ShortCode == shortCode);
    }
}
