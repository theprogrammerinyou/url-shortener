namespace UrlShortener.Core.DTOs;

public sealed class ClicksOverTimeResponse
{
    public IReadOnlyList<ClickTimePoint> Points { get; init; } = Array.Empty<ClickTimePoint>();
}

public sealed class ClickTimePoint
{
    public string Label { get; init; } = string.Empty;
    public int Clicks { get; init; }
}
