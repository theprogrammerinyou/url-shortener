using UrlShortener.Core.DTOs;

namespace UrlShortener.Core.Contracts;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request);
    Task<UserPreferencesResponse> GetPreferencesAsync(string userId);
    Task<UserPreferencesResponse> UpdatePreferencesAsync(UpdateUserPreferencesRequest request);
    Task<RegenerateApiKeyResponse> RegenerateApiKeyAsync(string userId);
}
