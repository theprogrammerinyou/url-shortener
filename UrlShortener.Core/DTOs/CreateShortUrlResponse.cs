namespace UrlShortener.Core.DTOs;

public sealed class CreateShortUrlResponse
{
    public string ShortUrl { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsCustom { get; init; }
    public string? UserId { get; init; }
}
