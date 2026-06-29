using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Utils;

namespace UrlShortener.Core.Services;

public sealed class UrlShortenerService : IUrlShortenerService
{
    private readonly IUrlRepository _repository;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IClickEventRepository _clickEventRepository;
    private readonly string _baseUrl;

    public UrlShortenerService(
        IUrlRepository repository,
        IKeyGenerator keyGenerator,
        IClickEventRepository clickEventRepository,
        string baseUrl)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _clickEventRepository = clickEventRepository ?? throw new ArgumentNullException(nameof(clickEventRepository));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
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

        var normalizedUrl = UrlUtils.NormalizeUrl(request.LongUrl);
        var now = DateTime.UtcNow;
        var expiresAt = request.ExpiresAt?.ToUniversalTime() ?? now.Add(TimeSpan.FromDays(30));

        if (!string.IsNullOrWhiteSpace(request.CustomAlias))
        {
            var customAlias = request.CustomAlias.Trim();
            UrlUtils.ValidateCustomAlias(customAlias);
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

        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
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
        await _clickEventRepository.AddAsync(new ClickEvent(shortCode, UrlUtils.NormalizeReferrer(referrer), country));
        return urlEntry.OriginalUrl;
    }

    public async Task<UrlDetailsResponse> GetDetailsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

        return await MapToDetailsResponseAsync(urlEntry);
    }

    public async Task<UrlAnalyticsResponse> GetAnalyticsAsync(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            throw new ArgumentException("Short code is required.", nameof(shortCode));
        }

        var urlEntry = await _repository.FindByShortCodeAsync(shortCode);
        if (urlEntry is null)
        {
            throw new UrlShortener.Core.Exceptions.NotFoundException("Short URL not found.");
        }

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

    private CreateShortUrlResponse MapToCreateResponse(UrlEntry urlEntry)
    {
        return new CreateShortUrlResponse
        {
            ShortCode = urlEntry.ShortCode,
            LongUrl = urlEntry.OriginalUrl,
            ShortUrl = UrlUtils.BuildShortUrl(urlEntry.ShortCode, _baseUrl),
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
            ShortUrl = UrlUtils.BuildShortUrl(urlEntry.ShortCode, _baseUrl),
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
}
