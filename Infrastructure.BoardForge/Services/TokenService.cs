using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.Endpoint.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevStack.Infrastructure.BoardForge.Services;

public class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions jwtOptions = jwtOptions.Value;

    public (string token, DateTime expiresAtUtc) GenerateToken(User user)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new("email_confirmed", user.EmailConfirmed.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
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
        return new RefreshTokenGeneratedDTO
        {
            RawToken = rawToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays),
            HashedToken = hashedToken
        };
    }

    public string ComputeHash(string raw)
    {
        var hashed = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(hashed);
    }
}