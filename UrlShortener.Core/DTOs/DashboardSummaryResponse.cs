namespace UrlShortener.Core.DTOs;

public sealed class DashboardSummaryResponse
{
    public double UptimePercentage { get; init; } = 99.9;
    public int AvgRedirectLatencyMs { get; init; } = 12;
    public long QrScansThisMonth { get; init; }
}
