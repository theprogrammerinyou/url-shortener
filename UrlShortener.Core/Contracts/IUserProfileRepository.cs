using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Contracts;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile> GetOrCreateAsync(string userId);
    Task UpdateAsync(UserProfile profile);
}
