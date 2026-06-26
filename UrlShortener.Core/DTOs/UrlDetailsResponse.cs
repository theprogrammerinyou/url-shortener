namespace UrlShortener.Core.DTOs;

public sealed class UrlDetailsResponse
{
    public string ShortCode { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsCustom { get; init; }
    public string? UserId { get; init; }
    public int ClickCount { get; init; }
    public int QrScanCount { get; init; }
    public bool IsExpired { get; init; }
    public bool IsPrivate { get; init; }
    public string Status { get; init; } = "Active";
    public int ClicksToday { get; init; }
}
