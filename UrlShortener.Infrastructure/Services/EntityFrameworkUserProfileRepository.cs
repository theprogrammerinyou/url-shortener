using UrlShortener.Core.Contracts;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class EntityFrameworkUserProfileRepository : IUserProfileRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public EntityFrameworkUserProfileRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.UserProfiles.FindAsync(userId);
    }

    public async Task<UserProfile> GetOrCreateAsync(string userId)
    {
        var existing = await GetByUserIdAsync(userId);
        if (existing is not null)
        {
            return existing;
        }

        var profile = new UserProfile(userId);
        await _dbContext.UserProfiles.AddAsync(profile);
        await _dbContext.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        _dbContext.UserProfiles.Update(profile);
        await _dbContext.SaveChangesAsync();
    }
}
