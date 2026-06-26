namespace UrlShortener.Core.DTOs;

public sealed class ShortenUrlResponse
{
    public string ShortUrl { get; init; } = string.Empty;
    public string OriginalUrl { get; init; } = string.Empty;
    public string ShortCode { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
