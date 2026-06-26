using UrlShortener.Core.DTOs;

namespace UrlShortener.Core.Contracts;

public interface IUrlShortenerService
{
    Task<CreateShortUrlResponse> CreateShortUrlAsync(CreateShortUrlRequest request);
    Task<string> ResolveAsync(string shortCode, string? referrer = null, string? country = null);
    Task<UrlDetailsResponse> GetDetailsAsync(string shortCode);
    Task<UrlAnalyticsResponse> GetAnalyticsAsync(string shortCode);
    Task<IEnumerable<UrlDetailsResponse>> GetAllUrlsAsync(string? userId = null);
    Task<PagedLinksResponse> GetPagedUrlsAsync(string? userId, string? search, string? status, int page, int pageSize);
    Task<IReadOnlyList<UrlDetailsResponse>> GetRecentUrlsAsync(string? userId, int limit = 5);
    Task DeleteUrlAsync(string shortCode, string? userId);
    Task RecordQrScanAsync(string shortCode);
}
