using System.Collections.Concurrent;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class InMemoryClickEventRepository : IClickEventRepository
{
    private readonly ConcurrentBag<ClickEvent> _events = new();

    public Task AddAsync(ClickEvent clickEvent)
    {
        _events.Add(clickEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ClickEvent>> GetByShortCodeAsync(string shortCode)
    {
        var results = _events.Where(x => x.ShortCode == shortCode).OrderByDescending(x => x.ClickedAt).ToList();
        return Task.FromResult<IReadOnlyList<ClickEvent>>(results);
    }

    public Task<IReadOnlyList<ClickEvent>> GetByUserIdAsync(string userId, DateTime? since = null)
    {
        var results = _events.AsEnumerable();
        if (since.HasValue)
        {
            results = results.Where(x => x.ClickedAt >= since.Value);
        }

        return Task.FromResult<IReadOnlyList<ClickEvent>>(results.OrderByDescending(x => x.ClickedAt).ToList());
    }

    public Task<IReadOnlyList<ClickEvent>> GetAllAsync(DateTime? since = null)
    {
        var results = _events.AsEnumerable();
        if (since.HasValue)
        {
            results = results.Where(x => x.ClickedAt >= since.Value);
        }

        return Task.FromResult<IReadOnlyList<ClickEvent>>(results.OrderByDescending(x => x.ClickedAt).ToList());
    }
}
