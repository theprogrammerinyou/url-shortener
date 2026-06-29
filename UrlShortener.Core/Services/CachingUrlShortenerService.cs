using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Utils;

namespace UrlShortener.Core.Services;

public sealed class CachingUrlShortenerService : IUrlShortenerService
{
    private readonly IUrlShortenerService _inner;
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CachingUrlShortenerService(IUrlShortenerService inner, IDistributedCache cache)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<CreateShortUrlResponse> CreateShortUrlAsync(CreateShortUrlRequest request)
    {
        var response = await _inner.CreateShortUrlAsync(request);
        
        var urlEntry = new UrlEntry(
            response.ShortCode,
            response.LongUrl,
            response.ExpiresAt,
            response.IsCustom,
            response.UserId
        );
        await SetEntryCacheAsync(urlEntry);
        return response;
    }

    public async Task<string> ResolveAsync(string shortCode, string? referrer = null, string? country = null)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            var urlEntry = new UrlEntry(
                cached.ShortCode,
                cached.OriginalUrl,
                cached.ExpiresAt,
                cached.IsCustom,
                cached.UserId,
                cached.ClickCount,
                cached.IsPrivate,
                cached.QrScanCount);

            if (urlEntry.IsExpired(DateTime.UtcNow))
            {
                throw new InvalidOperationException("Short URL has expired.");
            }

            // Fire-and-forget or background task to record the redirect in DB & analytics
            // so we don't block the critical path of redirecting the user.
            _ = Task.Run(async () =>
            {
                try
                {
                    await _inner.ResolveAsync(shortCode, referrer, country);
                    // Refresh cache with new click count
                    var latestDetails = await _inner.GetDetailsAsync(shortCode);
                    var entry = new UrlEntry(
                        latestDetails.ShortCode,
                        latestDetails.LongUrl,
                        latestDetails.ExpiresAt,
                        latestDetails.IsCustom,
                        latestDetails.UserId,
                        latestDetails.ClickCount,
                        latestDetails.IsPrivate,
                        latestDetails.QrScanCount);
                    await SetEntryCacheAsync(entry);
                }
                catch
                {
                    // Suppress exceptions in background task to avoid crash
                }
            });

            return urlEntry.OriginalUrl;
        }

        // Cache Miss
        var originalUrl = await _inner.ResolveAsync(shortCode, referrer, country);
        try
        {
            var latestDetails = await _inner.GetDetailsAsync(shortCode);
            var entry = new UrlEntry(
                latestDetails.ShortCode,
                latestDetails.LongUrl,
                latestDetails.ExpiresAt,
                latestDetails.IsCustom,
                latestDetails.UserId,
                latestDetails.ClickCount,
                latestDetails.IsPrivate,
                latestDetails.QrScanCount);
            await SetEntryCacheAsync(entry);
        }
        catch
        {
            // Do not block resolve if cache write fails
        }

        return originalUrl;
    }

    public async Task<UrlDetailsResponse> GetDetailsAsync(string shortCode)
    {
        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            // ClicksToday requires DB query, so we fetch it from inner and update cache metadata
            var details = await _inner.GetDetailsAsync(shortCode);
            var entry = new UrlEntry(
                details.ShortCode,
                details.LongUrl,
                details.ExpiresAt,
                details.IsCustom,
                details.UserId,
                details.ClickCount,
                details.IsPrivate,
                details.QrScanCount);
            await SetEntryCacheAsync(entry);
            return details;
        }

        var res = await _inner.GetDetailsAsync(shortCode);
        var urlEntry = new UrlEntry(
            res.ShortCode,
            res.LongUrl,
            res.ExpiresAt,
            res.IsCustom,
            res.UserId,
            res.ClickCount,
            res.IsPrivate,
            res.QrScanCount);
        await SetEntryCacheAsync(urlEntry);
        return res;
    }

    public async Task<UrlAnalyticsResponse> GetAnalyticsAsync(string shortCode)
    {
        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            return new UrlAnalyticsResponse
            {
                ShortCode = cached.ShortCode,
                LongUrl = cached.OriginalUrl,
                CreatedAt = cached.CreatedAt,
                ExpiresAt = cached.ExpiresAt,
                ClickCount = cached.ClickCount,
                IsExpired = cached.ExpiresAt < DateTime.UtcNow
            };
        }

        var res = await _inner.GetAnalyticsAsync(shortCode);
        return res;
    }

    public Task<IEnumerable<UrlDetailsResponse>> GetAllUrlsAsync(string? userId = null)
    {
        return _inner.GetAllUrlsAsync(userId);
    }

    public Task<PagedLinksResponse> GetPagedUrlsAsync(string? userId, string? search, string? status, int page, int pageSize)
    {
        return _inner.GetPagedUrlsAsync(userId, search, status, page, pageSize);
    }

    public Task<IReadOnlyList<UrlDetailsResponse>> GetRecentUrlsAsync(string? userId, int limit = 5)
    {
        return _inner.GetRecentUrlsAsync(userId, limit);
    }

    public async Task DeleteUrlAsync(string shortCode, string? userId)
    {
        await _inner.DeleteUrlAsync(shortCode, userId);
        await InvalidateCacheAsync(shortCode);
    }

    public async Task RecordQrScanAsync(string shortCode)
    {
        await _inner.RecordQrScanAsync(shortCode);
        await InvalidateCacheAsync(shortCode);
    }

    private async Task<CachedUrlEntry?> GetCachedUrlEntryAsync(string shortCode)
    {
        try
        {
            var cacheKey = GetEntryCacheKey(shortCode);
            var cached = await _cache.GetAsync(cacheKey);
            if (cached is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<CachedUrlEntry>(cached, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetEntryCacheAsync(UrlEntry urlEntry)
    {
        try
        {
            var cacheKey = GetEntryCacheKey(urlEntry.ShortCode);
            var options = new DistributedCacheEntryOptions();
            var now = DateTime.UtcNow;
            if (urlEntry.ExpiresAt > now)
            {
                options.AbsoluteExpiration = urlEntry.ExpiresAt;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }

            var cached = new CachedUrlEntry(
                urlEntry.ShortCode,
                urlEntry.OriginalUrl,
                urlEntry.CreatedAt,
                urlEntry.ExpiresAt,
                urlEntry.ClickCount,
                urlEntry.IsCustom,
                urlEntry.UserId,
                urlEntry.IsPrivate,
                urlEntry.QrScanCount);

            var payload = JsonSerializer.SerializeToUtf8Bytes(cached, JsonOptions);
            await _cache.SetAsync(cacheKey, payload, options);
        }
        catch
        {
            // Do not fail if caching operations fail
        }
    }

    private async Task InvalidateCacheAsync(string shortCode)
    {
        try
        {
            await _cache.RemoveAsync(GetEntryCacheKey(shortCode));
        }
        catch
        {
            // Do not fail if cache invalidation fails
        }
    }

    private static string GetEntryCacheKey(string shortCode) => $"urlentry:{shortCode}";

    private sealed record CachedUrlEntry(
        string ShortCode,
        string OriginalUrl,
        DateTime CreatedAt,
        DateTime ExpiresAt,
        int ClickCount,
        bool IsCustom,
        string? UserId,
        bool IsPrivate = false,
        int QrScanCount = 0);
}
