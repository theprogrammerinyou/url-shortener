using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Contracts;

public interface IUrlRepository
{
    Task AddAsync(UrlEntry urlEntry);
    Task<UrlEntry?> FindByShortCodeAsync(string shortCode);
    Task<UrlEntry?> FindByOriginalUrlAsync(string originalUrl);
    Task<IEnumerable<UrlEntry>> GetAllAsync(string? userId = null);
    Task<(IEnumerable<UrlEntry> Items, int TotalCount)> GetPagedAsync(
        string? userId,
        string? search,
        string? status,
        int page,
        int pageSize);
    Task<IReadOnlyList<UrlEntry>> GetRecentAsync(string? userId, int limit);
    Task DeleteAsync(string shortCode, string? userId);
    Task UpdateAsync(UrlEntry urlEntry);
    Task<bool> ShortCodeExistsAsync(string shortCode);
}
