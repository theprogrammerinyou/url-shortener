using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Core.Contracts;
using UrlShortener.Core.DTOs;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly UrlShortenerDbContext _db;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _expiryHours;

    public AuthService(
        UrlShortenerDbContext db,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience,
        int expiryHours = 720) // 30 days default
    {
        _db = db;
        _jwtKey = jwtKey;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
        _expiryHours = expiryHours;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate email is not taken
        var existing = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            Plan = "free"
        };

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        return IssueToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return IssueToken(user);
    }

    private AuthResponse IssueToken(AppUser user)
    {
        var expires = DateTime.UtcNow.AddHours(_expiryHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim("plan", user.Plan),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponse(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            UserId: user.Id.ToString(),
            Name: user.Name,
            Email: user.Email,
            Plan: user.Plan,
            ExpiresAt: expires
        );
    }
}
