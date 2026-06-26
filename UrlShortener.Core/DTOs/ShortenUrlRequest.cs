namespace UrlShortener.Core.DTOs;

public sealed class ShortenUrlRequest
{
    public string OriginalUrl { get; init; } = string.Empty;
}
