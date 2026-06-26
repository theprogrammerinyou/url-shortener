using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Contracts;

public interface IClickEventRepository
{
    Task AddAsync(ClickEvent clickEvent);
    Task<IReadOnlyList<ClickEvent>> GetByShortCodeAsync(string shortCode);
    Task<IReadOnlyList<ClickEvent>> GetByUserIdAsync(string userId, DateTime? since = null);
    Task<IReadOnlyList<ClickEvent>> GetAllAsync(DateTime? since = null);
}
