using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UrlShortener.Api.Middleware;

/// <summary>
/// Validates JWT Bearer tokens on protected routes and populates HttpContext.Items
/// with the authenticated user's claims. Public routes bypass validation.
/// </summary>
public sealed class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthMiddleware> _logger;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    // Routes that do NOT require a valid JWT
    private static readonly HashSet<string> _publicPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login",
        "/api/auth/register",
    };

    public JwtAuthMiddleware(
        RequestDelegate next,
        ILogger<JwtAuthMiddleware> logger,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience)
    {
        _next = next;
        _logger = logger;
        _jwtKey = jwtKey;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Short-link redirects (e.g. /abc123) and auth endpoints are public
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        var token = ExtractBearerToken(context);

        if (!string.IsNullOrEmpty(token))
        {
            var principal = ValidateToken(token);
            if (principal is not null)
            {
                // Attach claims to context so controllers can read them
                context.User = principal;
                var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var email  = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                var name   = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

                context.Items["AuthUserId"] = userId;
                context.Items["AuthEmail"]  = email;
                context.Items["AuthName"]   = name;

                _logger.LogDebug("Authenticated user {UserId} ({Email}) on {Path}", userId, email, path);
            }
            else
            {
                _logger.LogDebug("Invalid JWT token on {Path}", path);
            }
        }

        // We do NOT block unauthenticated requests here — controllers/endpoints
        // can use [Authorize] or check HttpContext.Items["AuthUserId"] themselves.
        // Guest users (no token) still get a pass for read-only operations.
        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        // Auth endpoints
        foreach (var prefix in _publicPrefixes)
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;

        // Short-link redirect: exactly /{shortCode} (no /api/ prefix)
        if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return authHeader["Bearer ".Length..].Trim();
    }

    private System.Security.Claims.ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtIssuer,
                ValidAudience = _jwtAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero,
            };
            return handler.ValidateToken(token, parameters, out _);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
