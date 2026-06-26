using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Services;

public sealed class UrlShortenerService : IUrlShortenerService
{
    private const int AliasLengthMin = 4;
    private const int AliasLengthMax = 100;
    private static readonly TimeSpan DefaultUrlLifetime = TimeSpan.FromDays(30);
    private static readonly Regex CustomAliasRegex = new($"^[a-zA-Z0-9_-]{{{AliasLengthMin},{AliasLengthMax}}}$", RegexOptions.Compiled);

    private readonly IUrlRepository _repository;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IClickEventRepository _clickEventRepository;
    private readonly string _baseUrl;
    private readonly IDistributedCache? _cache;

    public UrlShortenerService(
        IUrlRepository repository,
        IKeyGenerator keyGenerator,
        IClickEventRepository clickEventRepository,
        string baseUrl,
        IDistributedCache? cache = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _clickEventRepository = clickEventRepository ?? throw new ArgumentNullException(nameof(clickEventRepository));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _cache = cache;
    }

    public async Task<CreateShortUrlResponse> CreateShortUrlAsync(CreateShortUrlRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.LongUrl))
        {
            throw new ArgumentException("LongUrl is required.", nameof(request.LongUrl));
        }

        var normalizedUrl = NormalizeUrl(request.LongUrl);
        var now = DateTime.UtcNow;
        var expiresAt = request.ExpiresAt?.ToUniversalTime() ?? now.Add(DefaultUrlLifetime);

        if (!string.IsNullOrWhiteSpace(request.CustomAlias))
        {
            var customAlias = request.CustomAlias.Trim();
            ValidateCustomAlias(customAlias);
            if (await _repository.ShortCodeExistsAsync(customAlias))
            {
                throw new UrlShortener.Core.Exceptions.ConflictException("The custom alias is already in use.");
            }

            var customEntry = new UrlEntry(customAlias, normalizedUrl, expiresAt, isCustom: true, userId: request.UserId, isPrivate: request.IsPrivate);
            await _repository.AddAsync(customEntry);
            return MapToCreateResponse(customEntry);
        }

        var existing = await _repository.FindByOriginalUrlAsync(normalizedUrl);
        if (existing is not null && !existing.IsExpired(now))
        {
            return MapToCreateResponse(existing);
        }

        var shortCode = await GenerateUniqueShortCodeAsync(normalizedUrl);
        var generatedEntry = new UrlEntry(shortCode, normalizedUrl, expiresAt, isCustom: false, userId: request.UserId, isPrivate: request.IsPrivate);
        await _repository.AddAsync(generatedEntry);
        return MapToCreateResponse(generatedEntry);
    }

    public async Task<string> ResolveAsync(string shortCode, string? referrer = null, string? country = null)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var urlEntry = await GetEntryAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

        if (urlEntry.IsExpired(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Short URL has expired.");
        }

        urlEntry.IncrementClickCount();
        await _repository.UpdateAsync(urlEntry);
        await SetEntryCacheAsync(urlEntry);
        await _clickEventRepository.AddAsync(new ClickEvent(shortCode, NormalizeReferrer(referrer), country));
        return urlEntry.OriginalUrl;
    }

    public async Task<UrlDetailsResponse> GetDetailsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            return await MapToDetailsResponseAsync(cached);
        }

        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

        await SetEntryCacheAsync(urlEntry);
        return await MapToDetailsResponseAsync(urlEntry);
    }

    public async Task<UrlAnalyticsResponse> GetAnalyticsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            return MapToAnalyticsResponse(cached);
        }

        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

        await SetEntryCacheAsync(urlEntry);
        return MapToAnalyticsResponse(urlEntry);
    }

    public async Task<IEnumerable<UrlDetailsResponse>> GetAllUrlsAsync(string? userId = null)
    {
        var entries = await _repository.GetAllAsync(userId);
        var results = new List<UrlDetailsResponse>();
        foreach (var urlEntry in entries)
        {
            results.Add(await MapToDetailsResponseAsync(urlEntry));
        }

        return results;
    }

    public async Task<PagedLinksResponse> GetPagedUrlsAsync(string? userId, string? search, string? status, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _repository.GetPagedAsync(userId, search, status, page, pageSize);
        var mapped = new List<UrlDetailsResponse>();
        foreach (var urlEntry in items)
        {
            mapped.Add(await MapToDetailsResponseAsync(urlEntry));
        }

        return new PagedLinksResponse
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<UrlDetailsResponse>> GetRecentUrlsAsync(string? userId, int limit = 5)
    {
        var entries = await _repository.GetRecentAsync(userId, limit);
        var results = new List<UrlDetailsResponse>();
        foreach (var urlEntry in entries)
        {
            results.Add(await MapToDetailsResponseAsync(urlEntry));
        }

        return results;
    }

    public async Task DeleteUrlAsync(string shortCode, string? userId)
    {
        await _repository.DeleteAsync(shortCode, userId);
    }

    public async Task RecordQrScanAsync(string shortCode)
    {
        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

        urlEntry.IncrementQrScanCount();
        await _repository.UpdateAsync(urlEntry);
    }

    private async Task<string> GenerateUniqueShortCodeAsync(string input)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var shortCode = _keyGenerator.Generate(input);
            if (!await _repository.ShortCodeExistsAsync(shortCode))
            {
                return shortCode;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique short code.");
    }

    private async Task<UrlEntry?> GetEntryAsync(string shortCode)
    {
        var cached = await GetCachedUrlEntryAsync(shortCode);
        if (cached is not null)
        {
            return new UrlEntry(
                cached.ShortCode,
                cached.OriginalUrl,
                cached.ExpiresAt,
                cached.IsCustom,
                cached.UserId,
                cached.ClickCount,
                cached.IsPrivate,
                cached.QrScanCount);
        }

        return await _repository.FindByShortCodeAsync(shortCode);
    }

    private static string NormalizeUrl(string rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("A valid absolute URL is required.", nameof(rawUrl));
        }

        return uri.AbsoluteUri;
    }

    private static void ValidateCustomAlias(string customAlias)
    {
        if (!CustomAliasRegex.IsMatch(customAlias))
        {
            throw new ArgumentException($"Custom alias must be {AliasLengthMin}-{AliasLengthMax} characters and may contain letters, digits, underscores, or hyphens.", nameof(customAlias));
        }
    }

    private static string? NormalizeReferrer(string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer))
        {
            return "Direct / Unknown";
        }

        if (Uri.TryCreate(referrer, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return referrer;
    }

    private CreateShortUrlResponse MapToCreateResponse(UrlEntry urlEntry)
    {
        return new CreateShortUrlResponse
        {
            ShortCode = urlEntry.ShortCode,
            LongUrl = urlEntry.OriginalUrl,
            ShortUrl = BuildShortUrl(urlEntry.ShortCode),
            CreatedAt = urlEntry.CreatedAt,
            ExpiresAt = urlEntry.ExpiresAt,
            IsCustom = urlEntry.IsCustom,
            UserId = urlEntry.UserId
        };
    }

    private async Task<UrlDetailsResponse> MapToDetailsResponseAsync(UrlEntry urlEntry)
    {
        var now = DateTime.UtcNow;
        var isExpired = urlEntry.IsExpired(now);
        var clicksToday = await GetClicksTodayAsync(urlEntry.ShortCode);

        return new UrlDetailsResponse
        {
            ShortCode = urlEntry.ShortCode,
            LongUrl = urlEntry.OriginalUrl,
            ShortUrl = BuildShortUrl(urlEntry.ShortCode),
            CreatedAt = urlEntry.CreatedAt,
            ExpiresAt = urlEntry.ExpiresAt,
            IsCustom = urlEntry.IsCustom,
            UserId = urlEntry.UserId,
            ClickCount = urlEntry.ClickCount,
            QrScanCount = urlEntry.QrScanCount,
            IsExpired = isExpired,
            IsPrivate = urlEntry.IsPrivate,
            Status = isExpired ? "Expired" : urlEntry.IsPrivate ? "Private" : "Active",
            ClicksToday = clicksToday
        };
    }

    private async Task<UrlDetailsResponse> MapToDetailsResponseAsync(CachedUrlEntry cached)
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

        return await MapToDetailsResponseAsync(urlEntry);
    }

    private async Task<int> GetClicksTodayAsync(string shortCode)
    {
        var events = await _clickEventRepository.GetByShortCodeAsync(shortCode);
        var today = DateTime.UtcNow.Date;
        return events.Count(x => x.ClickedAt.Date == today);
    }

    private UrlAnalyticsResponse MapToAnalyticsResponse(UrlEntry urlEntry)
    {
        return new UrlAnalyticsResponse
        {
            ShortCode = urlEntry.ShortCode,
            LongUrl = urlEntry.OriginalUrl,
            CreatedAt = urlEntry.CreatedAt,
            ExpiresAt = urlEntry.ExpiresAt,
            ClickCount = urlEntry.ClickCount,
            IsExpired = urlEntry.IsExpired(DateTime.UtcNow)
        };
    }

    private UrlAnalyticsResponse MapToAnalyticsResponse(CachedUrlEntry cached)
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

    private async Task<CachedUrlEntry?> GetCachedUrlEntryAsync(string shortCode)
    {
        if (_cache is null)
        {
            return null;
        }

        var cacheKey = GetEntryCacheKey(shortCode);
        var cached = await _cache.GetAsync(cacheKey);
        if (cached is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CachedUrlEntry>(cached, JsonOptions);
    }

    private async Task SetEntryCacheAsync(UrlEntry urlEntry)
    {
        if (_cache is null)
        {
            return;
        }

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

    private static JsonSerializerOptions JsonOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static string GetEntryCacheKey(string shortCode) => $"urlentry:{shortCode}";

    private string BuildShortUrl(string shortCode)
    {
        return new Uri(new Uri(_baseUrl), shortCode).ToString();
    }

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
