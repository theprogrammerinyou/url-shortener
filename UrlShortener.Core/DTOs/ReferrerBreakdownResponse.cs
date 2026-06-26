namespace UrlShortener.Core.DTOs;

public sealed class ReferrerBreakdownResponse
{
    public IReadOnlyList<ReferrerItem> Referrers { get; init; } = Array.Empty<ReferrerItem>();
}

public sealed class ReferrerItem
{
    public string Name { get; init; } = string.Empty;
    public int Clicks { get; init; }
    public double Percentage { get; init; }
}
