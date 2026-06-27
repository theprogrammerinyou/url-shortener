using UrlShortener.Core.DTOs;

namespace UrlShortener.Core.Contracts;

public interface IDashboardService
{
    Task<DashboardStatsResponse> GetStatsAsync(string? userId);
    Task<DashboardSummaryResponse> GetSummaryAsync(string? userId);
    Task<LinkVelocityResponse> GetVelocityAsync(string? userId);
}
