using UrlShortener.Core.DTOs;

namespace UrlShortener.Core.Contracts;

public interface IAnalyticsService
{
    Task<LinkAnalyticsDetailResponse> GetLinkAnalyticsAsync(string shortCode);
    Task<ClicksOverTimeResponse> GetClicksOverTimeAsync(string shortCode, string period);
    Task<ReferrerBreakdownResponse> GetReferrersAsync(string shortCode);
    Task<GeoDistributionResponse> GetGeoDistributionAsync(string shortCode);
}
