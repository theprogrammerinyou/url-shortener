namespace UrlShortener.Core.DTOs;

public sealed class PagedLinksResponse
{
    public IReadOnlyList<UrlDetailsResponse> Items { get; init; } = Array.Empty<UrlDetailsResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
