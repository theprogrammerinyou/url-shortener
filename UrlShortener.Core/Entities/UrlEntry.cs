namespace UrlShortener.Core.Entities;

public sealed class UrlEntry
{
    public string ShortCode { get; init; } = string.Empty;
    public string OriginalUrl { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
    public int ClickCount { get; private set; }
    public int QrScanCount { get; private set; }
    public bool IsCustom { get; init; }
    public bool IsPrivate { get; init; }

    public UrlEntry(string shortCode, string originalUrl, DateTime expiresAt, bool isCustom = false, string? userId = null, int clickCount = 0, bool isPrivate = false, int qrScanCount = 0)
    {
        ShortCode = shortCode ?? throw new ArgumentNullException(nameof(shortCode));
        OriginalUrl = originalUrl ?? throw new ArgumentNullException(nameof(originalUrl));
        ExpiresAt = expiresAt;
        IsCustom = isCustom;
        UserId = userId;
        ClickCount = clickCount;
        IsPrivate = isPrivate;
        QrScanCount = qrScanCount;
    }

    public void IncrementQrScanCount()
    {
        QrScanCount++;
    }

    public void IncrementClickCount()
    {
        ClickCount++;
    }

    public bool IsExpired(DateTime now)
    {
        return ExpiresAt < now;
    }
}
