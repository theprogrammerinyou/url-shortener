namespace UrlShortener.Core.Entities;

public sealed class ClickEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShortCode { get; init; } = string.Empty;
    public DateTime ClickedAt { get; init; } = DateTime.UtcNow;
    public string? Referrer { get; init; }
    public string? Country { get; init; }

    public ClickEvent(string shortCode, string? referrer = null, string? country = null)
    {
        ShortCode = shortCode ?? throw new ArgumentNullException(nameof(shortCode));
        Referrer = referrer;
        Country = country;
    }
}
