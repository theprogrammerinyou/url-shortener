using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class EntityFrameworkClickEventRepository : IClickEventRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public EntityFrameworkClickEventRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(ClickEvent clickEvent)
    {
        await _dbContext.ClickEvents.AddAsync(clickEvent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ClickEvent>> GetByShortCodeAsync(string shortCode)
    {
        return await _dbContext.ClickEvents
            .Where(x => x.ShortCode == shortCode)
            .OrderByDescending(x => x.ClickedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClickEvent>> GetByUserIdAsync(string userId, DateTime? since = null)
    {
        var query = from click in _dbContext.ClickEvents
                    join mapping in _dbContext.UrlMappings on click.ShortCode equals mapping.ShortCode
                    where mapping.UserId == userId
                    select click;

        if (since.HasValue)
        {
            query = query.Where(x => x.ClickedAt >= since.Value);
        }

        return await query.OrderByDescending(x => x.ClickedAt).ToListAsync();
    }

    public async Task<IReadOnlyList<ClickEvent>> GetAllAsync(DateTime? since = null)
    {
        var query = _dbContext.ClickEvents.AsQueryable();
        if (since.HasValue)
        {
            query = query.Where(x => x.ClickedAt >= since.Value);
        }

        return await query.OrderByDescending(x => x.ClickedAt).ToListAsync();
    }
}
