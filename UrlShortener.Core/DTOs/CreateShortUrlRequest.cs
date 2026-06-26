namespace UrlShortener.Core.DTOs;

public sealed class CreateShortUrlRequest
{
    public string LongUrl { get; init; } = string.Empty;
    public string? CustomAlias { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? UserId { get; init; }
    public bool GenerateQrCode { get; init; }
    public bool IsPrivate { get; init; }
}
