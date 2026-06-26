namespace UrlShortener.Core.DTOs;

public sealed class DashboardStatsResponse
{
    public long TotalClicks { get; init; }
    public int ActiveLinks { get; init; }
    public int LinkLimit { get; init; } = 200;
    public string TopReferral { get; init; } = "Direct";
    public double TopReferralPercentage { get; init; }
    public double ClickGrowthPercentage { get; init; }
}
