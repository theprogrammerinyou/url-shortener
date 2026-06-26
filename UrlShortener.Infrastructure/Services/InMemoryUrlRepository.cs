using System.Collections.Concurrent;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class InMemoryUrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, UrlEntry> _urls = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(UrlEntry urlEntry)
    {
        if (urlEntry == null)
        {
            throw new ArgumentNullException(nameof(urlEntry));
        }

        var added = _urls.TryAdd(urlEntry.ShortCode, urlEntry);
        if (!added)
        {
            throw new InvalidOperationException("A URL with the specified short code already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<UrlEntry?> FindByShortCodeAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        _urls.TryGetValue(shortCode, out var urlEntry);
        return Task.FromResult(urlEntry);
    }

    public Task<UrlEntry?> FindByOriginalUrlAsync(string originalUrl)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            throw new ArgumentException("Original URL is required.", nameof(originalUrl));
        }

        var urlEntry = _urls.Values.FirstOrDefault(x => string.Equals(x.OriginalUrl, originalUrl, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(urlEntry);
    }

    public Task<IEnumerable<UrlEntry>> GetAllAsync(string? userId = null)
    {
        var values = _urls.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            values = values.Where(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IEnumerable<UrlEntry>>(values.OrderBy(x => x.CreatedAt).ToArray());
    }

    public Task<(IEnumerable<UrlEntry> Items, int TotalCount)> GetPagedAsync(
        string? userId,
        string? search,
        string? status,
        int page,
        int pageSize)
    {
        var values = _urls.Values.AsEnumerable();
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            values = values.Where(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            values = values.Where(x =>
                x.ShortCode.ToLowerInvariant().Contains(term) ||
                x.OriginalUrl.ToLowerInvariant().Contains(term));
        }

        status = status?.Trim().ToLowerInvariant();
        values = status switch
        {
            "active" => values.Where(x => x.ExpiresAt >= now),
            "expired" => values.Where(x => x.ExpiresAt < now),
            "private" => values.Where(x => x.IsPrivate),
            _ => values
        };

        var list = values.OrderByDescending(x => x.CreatedAt).ToList();
        var items = list.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize);
        return Task.FromResult<(IEnumerable<UrlEntry>, int)>((items, list.Count));
    }

    public Task<IReadOnlyList<UrlEntry>> GetRecentAsync(string? userId, int limit)
    {
        var values = _urls.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            values = values.Where(x => string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<UrlEntry>>(
            values.OrderByDescending(x => x.CreatedAt).Take(limit).ToList());
    }

    public Task DeleteAsync(string shortCode, string? userId)
    {
        if (!_urls.TryGetValue(shortCode, out var urlEntry))
        {
            return Task.CompletedTask;
        }

        if (!string.IsNullOrWhiteSpace(userId) && urlEntry.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this link.");
        }

        _urls.TryRemove(shortCode, out _);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UrlEntry urlEntry)
    {
        if (urlEntry == null)
        {
            throw new ArgumentNullException(nameof(urlEntry));
        }

        if (!_urls.ContainsKey(urlEntry.ShortCode))
        {
            throw new InvalidOperationException("Unable to update URL entry because it does not exist.");
        }

        _urls[urlEntry.ShortCode] = urlEntry;
        return Task.CompletedTask;
    }

    public Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        return Task.FromResult(_urls.ContainsKey(shortCode));
    }
}
