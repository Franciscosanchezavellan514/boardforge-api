using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Models;
using DevStack.Infrastructure.BoardForge.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace DevStack.Infrastructure.BoardForge.Tests.Services;

[TestClass]
public class TokenServiceTests
{
    private readonly Mock<IOptions<JwtOptions>> _optionsMock;
    private readonly FakeTimeProvider fakeTimeProvider;
    private readonly JwtOptions _jwtOptions;
    private ITokenService _service;

    public TokenServiceTests()
    {
        _optionsMock = new Mock<IOptions<JwtOptions>>();
        _jwtOptions = CreateTestJwtOptions();
        _optionsMock.Setup(o => o.Value).Returns(_jwtOptions);
        fakeTimeProvider = new FakeTimeProvider();
        _service = new TokenService(_optionsMock.Object, fakeTimeProvider);
    }

    public static User CreateTestUser() => new()
    {
        Id = 1,
        DisplayName = "Test User",
        Email = "testuser@example.com",
        EmailConfirmed = true,
    };

    private static JwtOptions CreateTestJwtOptions() => new()
    {
        AccessTokenMinutes = 60,
        Audience = "test_audience",
        Issuer = "test_issuer",
        RefreshTokenDays = 7,
        SigningKey = "test_signing_key_of_sufficient_length_12345" // Ensure key length is sufficient (at least 32 characters for HmacSha256)
    };

    [TestMethod]
    public void GenerateToken_Should_ReturnValidToken()
    {
        // Arrange
        User user = CreateTestUser();
        _optionsMock.Setup(o => o.Value).Returns(_jwtOptions);

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.IsNotNull(token);
    }

    [TestMethod]
    public void GenerateToken_Should_HaveValidSignature()
    {
        // Arrange
        User user = CreateTestUser();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        fakeTimeProvider.SetUtcNow(DateTime.UtcNow);

        // Act
        var (tokenString, _) = _service.GenerateToken(user);

        // Assert
        tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        Assert.IsNotNull(validatedToken);
        Assert.IsInstanceOfType(validatedToken, typeof(JwtSecurityToken));
        Assert.AreEqual(_jwtOptions.Issuer, ((JwtSecurityToken)validatedToken).Issuer);
        Assert.AreEqual(_jwtOptions.Audience, ((JwtSecurityToken)validatedToken).Audiences.First());
    }

    [TestMethod]
    public void GenerateToken_Should_HaveValidPrincipal()
    {
        // Arrange
        User user = CreateTestUser();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        fakeTimeProvider.SetUtcNow(DateTime.UtcNow);

        // Act
        var (tokenString, _) = _service.GenerateToken(user);

        // Assert
        ClaimsPrincipal principal = tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        Assert.IsNotNull(validatedToken);

        Assert.AreEqual(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.AreEqual(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.AreEqual(user.DisplayName, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.AreEqual(user.EmailConfirmed.ToString(), principal.FindFirst("email_confirmed")?.Value);
    }

    [TestMethod]
    public void GenerateToken_Should_SetCorrectExpiration()
    {
        // Arrange
        User user = CreateTestUser();
        DateTime fakeNow = new(2025, 9, 11, 1, 49, 0, DateTimeKind.Utc);
        fakeTimeProvider.SetUtcNow(fakeNow);
        var tokenHandler = new JwtSecurityTokenHandler();

        // Act
        var (tokenString, expiresAtUtc) = _service.GenerateToken(user);

        // Assert
        Assert.AreEqual(fakeNow.AddMinutes(_jwtOptions.AccessTokenMinutes), expiresAtUtc);

        var jwtToken = tokenHandler.ReadJwtToken(tokenString);
        Assert.AreEqual(fakeNow.AddMinutes(_jwtOptions.AccessTokenMinutes), jwtToken.ValidTo);
    }

    [TestMethod]
    public void GenerateRefreshToken_Should_ReturnValidToken()
    {
        // Arrange
        DateTime fakeNow = new(2025, 9, 11, 1, 49, 0, DateTimeKind.Utc);
        fakeTimeProvider.SetUtcNow(fakeNow);

        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        Assert.IsNotNull(refreshToken);
        Assert.IsFalse(string.IsNullOrEmpty(refreshToken.RawToken));
        Assert.IsFalse(string.IsNullOrEmpty(refreshToken.HashedToken));
        Assert.AreEqual(fakeNow.AddDays(_jwtOptions.RefreshTokenDays), refreshToken.ExpiresAtUtc);
    }

    [TestMethod]
    public void ComputeHash_Should_ReturnConsistentHash()
    {
        // Arrange
        string rawToken = "sample_raw_token";
        string expectedHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        // Act
        string computedHash = _service.ComputeHash(rawToken);
        string recomputedHash = _service.ComputeHash(rawToken);
        string differentHash = _service.ComputeHash("different_raw_token");

        // Assert
        Assert.AreEqual(expectedHash, computedHash);
        Assert.AreEqual(expectedHash, recomputedHash);
        Assert.AreNotEqual(expectedHash, differentHash);
    }
}