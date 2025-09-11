using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevStack.Infrastructure.BoardForge.Services;

public class TokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public (string token, DateTime expiresAtUtc) GenerateToken(User user)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        DateTime expiresAtUtc = _timeProvider.GetUtcNow()
            .AddMinutes(_jwtOptions.AccessTokenMinutes)
            .UtcDateTime;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("email_confirmed", user.EmailConfirmed.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public RefreshTokenGeneratedDTO GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(bytes);
        var hashedToken = ComputeHash(rawToken);
        DateTimeOffset expiresAt = _timeProvider.GetUtcNow().AddDays(_jwtOptions.RefreshTokenDays);
        return new RefreshTokenGeneratedDTO
        {
            RawToken = rawToken,
            ExpiresAtUtc = expiresAt.UtcDateTime,
            HashedToken = hashedToken
        };
    }

    public string ComputeHash(string raw)
    {
        var hashed = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(hashed);
    }
}