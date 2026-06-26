using UrlShortener.Core.DTOs;

namespace UrlShortener.Core.Contracts;

public interface IDashboardService
{
    Task<DashboardStatsResponse> GetStatsAsync(string? userId);
    Task<DashboardSummaryResponse> GetSummaryAsync(string? userId);
    Task<LinkVelocityResponse> GetVelocityAsync(string? userId);
}

public interface IAnalyticsService
{
    Task<LinkAnalyticsDetailResponse> GetLinkAnalyticsAsync(string shortCode);
    Task<ClicksOverTimeResponse> GetClicksOverTimeAsync(string shortCode, string period);
    Task<ReferrerBreakdownResponse> GetReferrersAsync(string shortCode);
    Task<GeoDistributionResponse> GetGeoDistributionAsync(string shortCode);
}

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request);
    Task<UserPreferencesResponse> GetPreferencesAsync(string userId);
    Task<UserPreferencesResponse> UpdatePreferencesAsync(UpdateUserPreferencesRequest request);
    Task<RegenerateApiKeyResponse> RegenerateApiKeyAsync(string userId);
}
