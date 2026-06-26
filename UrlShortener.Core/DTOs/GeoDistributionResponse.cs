namespace UrlShortener.Core.DTOs;

public sealed class GeoDistributionResponse
{
    public IReadOnlyList<GeoItem> Countries { get; init; } = Array.Empty<GeoItem>();
}

public sealed class GeoItem
{
    public string Country { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public int Clicks { get; init; }
    public double Percentage { get; init; }
}
