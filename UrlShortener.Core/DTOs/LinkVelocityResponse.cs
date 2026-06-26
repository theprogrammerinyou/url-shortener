namespace UrlShortener.Core.DTOs;

public sealed class LinkVelocityResponse
{
    public IReadOnlyList<LinkVelocityPoint> Points { get; init; } = Array.Empty<LinkVelocityPoint>();
}

public sealed class LinkVelocityPoint
{
    public string Label { get; init; } = string.Empty;
    public int Clicks { get; init; }
}
