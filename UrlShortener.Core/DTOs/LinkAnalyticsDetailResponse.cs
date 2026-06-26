namespace UrlShortener.Core.DTOs;

public sealed class LinkAnalyticsDetailResponse
{
    public string ShortCode { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public bool IsActive { get; init; }
    public int TotalEngagement { get; init; }
    public double EngagementGrowthPercentage { get; init; }
    public double ConversionRate { get; init; }
    public int Conversions { get; init; }
}
