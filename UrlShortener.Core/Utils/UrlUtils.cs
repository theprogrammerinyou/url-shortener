using System.Text.RegularExpressions;

namespace UrlShortener.Core.Utils;

public static class UrlUtils
{
    private const int AliasLengthMin = 4;
    private const int AliasLengthMax = 100;
    private static readonly Regex CustomAliasRegex = new($"^[a-zA-Z0-9_-]{{{AliasLengthMin},{AliasLengthMax}}}$", RegexOptions.Compiled);

    public static string NormalizeUrl(string rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("A valid absolute URL is required.", nameof(rawUrl));
        }

        return uri.AbsoluteUri;
    }

    public static void ValidateCustomAlias(string customAlias)
    {
        if (!CustomAliasRegex.IsMatch(customAlias))
        {
            throw new ArgumentException($"Custom alias must be {AliasLengthMin}-{AliasLengthMax} characters and may contain letters, digits, underscores, or hyphens.", nameof(customAlias));
        }
    }

    public static string? NormalizeReferrer(string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer))
        {
            return "Direct / Unknown";
        }

        if (Uri.TryCreate(referrer, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return referrer;
    }

    public static string BuildShortUrl(string shortCode, string baseUrl)
    {
        return new Uri(new Uri(baseUrl), shortCode).ToString();
    }
}
