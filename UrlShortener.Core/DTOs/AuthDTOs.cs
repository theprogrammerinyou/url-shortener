namespace UrlShortener.Core.DTOs;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string Token,
    string UserId,
    string Name,
    string Email,
    string Plan,
    DateTime ExpiresAt
);
