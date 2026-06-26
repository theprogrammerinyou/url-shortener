namespace UrlShortener.Core.DTOs;

public sealed class UrlAnalyticsResponse
{
    public string ShortCode { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public int ClickCount { get; init; }
    public bool IsExpired { get; init; }
}
