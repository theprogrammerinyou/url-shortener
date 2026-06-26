namespace UrlShortener.Core.DTOs;

public sealed class UserProfileResponse
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DefaultDomain { get; init; } = string.Empty;
    public string MaskedApiKey { get; init; } = string.Empty;
    public string Plan { get; init; } = "Free";
}

public sealed class UpdateUserProfileRequest
{
    public string UserId { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? DefaultDomain { get; init; }
}

public sealed class UserPreferencesResponse
{
    public string Theme { get; init; } = "light";
    public bool WeeklyAnalyticsReport { get; init; }
    public bool LinkThresholdAlerts { get; init; }
    public bool NewDeviceLogin { get; init; }
    public bool CompactView { get; init; }
}

public sealed class UpdateUserPreferencesRequest
{
    public string UserId { get; init; } = string.Empty;
    public string? Theme { get; init; }
    public bool? WeeklyAnalyticsReport { get; init; }
    public bool? LinkThresholdAlerts { get; init; }
    public bool? NewDeviceLogin { get; init; }
    public bool? CompactView { get; init; }
}

public sealed class RegenerateApiKeyResponse
{
    public string MaskedApiKey { get; init; } = string.Empty;
}
