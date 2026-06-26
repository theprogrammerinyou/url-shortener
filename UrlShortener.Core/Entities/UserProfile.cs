namespace UrlShortener.Core.Entities;

public sealed class UserProfile
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; set; } = "John Doe";
    public string Email { get; set; } = string.Empty;
    public string DefaultDomain { get; set; } = "l.swift";
    public string ApiKey { get; set; } = string.Empty;
    public string Theme { get; set; } = "light";
    public bool WeeklyAnalyticsReport { get; set; } = true;
    public bool LinkThresholdAlerts { get; set; } = true;
    public bool NewDeviceLogin { get; set; }
    public bool CompactView { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public UserProfile(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Email = $"{userId[..Math.Min(8, userId.Length)]}@linkswift.io";
        ApiKey = GenerateApiKey();
    }

    private static string GenerateApiKey() =>
        $"ls_{Guid.NewGuid():N}{Guid.NewGuid():N}"[..40];
}
