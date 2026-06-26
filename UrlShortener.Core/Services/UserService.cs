using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Services;

public sealed class UserService : IUserService
{
    private readonly IUserProfileRepository _repository;

    public UserService(IUserProfileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var profile = await _repository.GetOrCreateAsync(userId);
        return MapProfile(profile);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(UpdateUserProfileRequest request)
    {
        var profile = await _repository.GetOrCreateAsync(request.UserId);

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            profile.DisplayName = request.DisplayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            profile.Email = request.Email.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.DefaultDomain))
        {
            profile.DefaultDomain = request.DefaultDomain.Trim();
        }

        await _repository.UpdateAsync(profile);
        return MapProfile(profile);
    }

    public async Task<UserPreferencesResponse> GetPreferencesAsync(string userId)
    {
        var profile = await _repository.GetOrCreateAsync(userId);
        return MapPreferences(profile);
    }

    public async Task<UserPreferencesResponse> UpdatePreferencesAsync(UpdateUserPreferencesRequest request)
    {
        var profile = await _repository.GetOrCreateAsync(request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Theme))
        {
            profile.Theme = request.Theme;
        }

        if (request.WeeklyAnalyticsReport.HasValue)
        {
            profile.WeeklyAnalyticsReport = request.WeeklyAnalyticsReport.Value;
        }

        if (request.LinkThresholdAlerts.HasValue)
        {
            profile.LinkThresholdAlerts = request.LinkThresholdAlerts.Value;
        }

        if (request.NewDeviceLogin.HasValue)
        {
            profile.NewDeviceLogin = request.NewDeviceLogin.Value;
        }

        if (request.CompactView.HasValue)
        {
            profile.CompactView = request.CompactView.Value;
        }

        await _repository.UpdateAsync(profile);
        return MapPreferences(profile);
    }

    public async Task<RegenerateApiKeyResponse> RegenerateApiKeyAsync(string userId)
    {
        var profile = await _repository.GetOrCreateAsync(userId);
        profile.ApiKey = $"ls_{Guid.NewGuid():N}{Guid.NewGuid():N}"[..40];
        await _repository.UpdateAsync(profile);

        return new RegenerateApiKeyResponse
        {
            MaskedApiKey = MaskApiKey(profile.ApiKey)
        };
    }

    private static UserProfileResponse MapProfile(UserProfile profile) =>
        new()
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Email = profile.Email,
            DefaultDomain = profile.DefaultDomain,
            MaskedApiKey = MaskApiKey(profile.ApiKey),
            Plan = "Pro"
        };

    private static UserPreferencesResponse MapPreferences(UserProfile profile) =>
        new()
        {
            Theme = profile.Theme,
            WeeklyAnalyticsReport = profile.WeeklyAnalyticsReport,
            LinkThresholdAlerts = profile.LinkThresholdAlerts,
            NewDeviceLogin = profile.NewDeviceLogin,
            CompactView = profile.CompactView
        };

    private static string MaskApiKey(string apiKey)
    {
        if (apiKey.Length <= 8)
        {
            return "••••••••";
        }

        return $"{apiKey[..4]}••••••••{apiKey[^4..]}";
    }
}
