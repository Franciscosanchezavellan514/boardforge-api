using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.Services;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace DevStack.Application.BoardForge.Tests.Services;

[TestClass]
public class AuthenticationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _authenticationService = new AuthenticationService(
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _fakeTimeProvider
        );
    }

    [TestCleanup]
    public void Cleanup()
    {
        _unitOfWorkMock.Reset();
        _tokenServiceMock.Reset();
        _passwordHasherMock.Reset();
    }

    private static AuthenticateUserRequest CreateAuthenticateUserRequest() => new()
    {
        Email = "tester@example.com",
        Password = "Password123!",
        DeviceName = "TestDevice",
        IpAddress = "127.0.0.1",
        UserAgent = "UnitTestAgent"
    };

    #region AuthenticateAsync Tests

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task AuthenticateAsync__ShouldThrowArgumentException_WhenEmailIsNullOrEmpty(string email)
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        request.Email = email;

        // Act
        Task act() => _authenticationService.AuthenticateAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(act);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task AuthenticateAsync__ShouldThrowArgumentException_WhenPasswordIsNullOrEmpty(string password)
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        request.Password = password;

        // Act
        Task act() => _authenticationService.AuthenticateAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(act);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act
        Task act() => _authenticationService.AuthenticateAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _unitOfWorkMock.Verify(u => u.Users.GetByEmailAsync(request.Email.ToLower().Trim()), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldNotCallPasswordHasher_WhenUserNotFound()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act
        Task act() => _authenticationService.AuthenticateAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _passwordHasherMock.Verify(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        User user = new()
        {
            Id = 1,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = "hashedPassword",
            Salt = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
        };
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(false);

        // Act
        Task act() => _authenticationService.AuthenticateAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _unitOfWorkMock.Verify(u => u.Users.GetByEmailAsync(request.Email.ToLower().Trim()), Times.Once);
        _passwordHasherMock.Verify(
            p => p.VerifyHashedPassword(
                user.PasswordHash,
                request.Password,
                Convert.FromBase64String(user.Salt)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldReturnTokenResponseDTO_WhenCredentialsAreValid()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        User user = new()
        {
            Id = 1,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = "hashedPassword",
            Salt = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
        };
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        var tokenResponse = ("jwtToken", currentTime.AddDays(1).UtcDateTime);
        var refreshTokenResponse = new RefreshTokenGeneratedDTO
        {
            RawToken = "rawRefreshToken",
            HashedToken = "hashedRefreshToken",
            ExpiresAtUtc = currentTime.AddDays(7).UtcDateTime
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.RefreshTokens.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(tokenResponse);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(refreshTokenResponse);
        _fakeTimeProvider.SetUtcNow(currentTime);

        TokenResponseDTO expectedResponse = new()
        {
            Token = tokenResponse.Item1,
            TokenExpiresIn = tokenResponse.Item2,
            RefreshToken = refreshTokenResponse.RawToken,
            RefreshTokenExpiresIn = refreshTokenResponse.ExpiresAtUtc
        };

        // Act
        TokenResponseDTO response = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(expectedResponse.Token, response.Token);
        Assert.AreEqual(expectedResponse.TokenExpiresIn, response.TokenExpiresIn);
        Assert.AreEqual(expectedResponse.RefreshToken, response.RefreshToken);
        Assert.AreEqual(expectedResponse.RefreshTokenExpiresIn, response.RefreshTokenExpiresIn);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldCallGenerateToken_WhenCredentialsAreValid()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        AuthenticateAsyncSetupHappyPath(request, out User user, out RefreshToken _);
        // Act
        await _authenticationService.AuthenticateAsync(request);

        // Assert
        _tokenServiceMock.Verify(t => t.GenerateToken(user), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldCallRefreshTokenOnce_WhenCredentialsAreValid()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        AuthenticateAsyncSetupHappyPath(request, out User _, out RefreshToken _);

        // Act
        await _authenticationService.AuthenticateAsync(request);

        // Assert
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
    }

    [TestMethod]
    public async Task AuthenticateAsync__ShouldStoreRefreshToken_WhenCredentialsAreValid()
    {
        // Arrange
        AuthenticateUserRequest request = CreateAuthenticateUserRequest();
        AuthenticateAsyncSetupHappyPath(request, out User _, out RefreshToken expectedRefreshToken);

        // Act
        await _authenticationService.AuthenticateAsync(request);

        // Assert
        _unitOfWorkMock.Verify(u => u.RefreshTokens.AddAsync(
            It.Is<RefreshToken>(rt =>
                rt.TokenHash == expectedRefreshToken.TokenHash &&
                rt.UserId == expectedRefreshToken.UserId &&
                rt.CreatedByIp == expectedRefreshToken.CreatedByIp &&
                rt.UserAgent == expectedRefreshToken.UserAgent &&
                rt.DeviceName == expectedRefreshToken.DeviceName &&
                rt.ExpiresAtUtc == expectedRefreshToken.ExpiresAtUtc &&
                rt.CreatedAtUtc == expectedRefreshToken.CreatedAtUtc
            )
        ), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    private void AuthenticateAsyncSetupHappyPath(AuthenticateUserRequest request, out User user, out RefreshToken refreshToken)
    {
        user = new()
        {
            Id = 1,
            Email = request.Email.ToLower().Trim(),
            PasswordHash = "hashedPassword",
            Salt = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
        };
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        var tokenResponse = ("jwtToken", currentTime.AddDays(1).UtcDateTime);
        var refreshTokenResponse = new RefreshTokenGeneratedDTO
        {
            RawToken = "rawRefreshToken",
            HashedToken = "hashedRefreshToken",
            ExpiresAtUtc = currentTime.AddDays(7).UtcDateTime
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.RefreshTokens.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(tokenResponse);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken())
            .Returns(refreshTokenResponse);
        _fakeTimeProvider.SetUtcNow(currentTime);

        refreshToken = new()
        {
            TokenHash = refreshTokenResponse.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = refreshTokenResponse.ExpiresAtUtc,
            CreatedAtUtc = currentTime.UtcDateTime
        };
    }

    #endregion AuthenticateAsync Tests

    #region RefreshTokenAsync Tests

    [TestMethod]
    public async Task RefreshTokenAsync__ShouldThrow_UnauthorizedAccessException_WhenRefreshTokenIsNotFound()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");

        // Act
        Task act() => _authenticationService.RefreshTokenAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _tokenServiceMock.Verify(t => t.ComputeHash(refreshToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.RefreshTokens.GetByTokenAsync("hashedRefreshToken"), Times.Once);
    }


    [TestMethod]
    public async Task RefreshTokenAsync__ShouldThrow_UnauthorizedAccessException_WhenRefreshTokenIsRevoked()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            RevokedAtUtc = DateTime.UtcNow.AddDays(5),
        };
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);

        // Act
        Task act() => _authenticationService.RefreshTokenAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _tokenServiceMock.Verify(t => t.ComputeHash(refreshToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.RefreshTokens.GetByTokenAsync("hashedRefreshToken"), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync__ShouldThrow_UnauthorizedAccessException_WhenRefreshTokenIsExpired()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            ExpiresAtUtc = currentTime.AddSeconds(-1).UtcDateTime,
        };
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _fakeTimeProvider.SetUtcNow(currentTime);

        // Act
        Task act() => _authenticationService.RefreshTokenAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(act);
        _tokenServiceMock.Verify(t => t.ComputeHash(refreshToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.RefreshTokens.GetByTokenAsync("hashedRefreshToken"), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync__ShouldThrow_EntityNotFoundException_WhenUserIsNotFound()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            UserId = 1,
            ExpiresAtUtc = currentTime.AddSeconds(1).UtcDateTime,
        };
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);
        _fakeTimeProvider.SetUtcNow(currentTime);

        // Act
        Task act() => _authenticationService.RefreshTokenAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException>(act);
        _tokenServiceMock.Verify(t => t.ComputeHash(refreshToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.RefreshTokens.GetByTokenAsync("hashedRefreshToken"), Times.Once);
        _unitOfWorkMock.Verify(u => u.Users.GetByIdAsync(storedToken.UserId), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync__ShouldCall_GenerateTokenAndRefreshTokenAsync_WhenRefreshTokenIsValid()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            UserId = 1,
            ExpiresAtUtc = currentTime.AddSeconds(1).UtcDateTime,
        };
        User user = new()
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = "hashedPassword"
        };
        var tokenResponse = ("jwtToken", currentTime.AddDays(1).UtcDateTime);
        var refreshTokenResponse = new RefreshTokenGeneratedDTO
        {
            RawToken = "rawRefreshToken",
            HashedToken = "hashedRefreshToken",
            ExpiresAtUtc = currentTime.AddDays(7).UtcDateTime
        };
        _unitOfWorkMock.SetupAllProperties();
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(tokenResponse);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(refreshTokenResponse);

        // Act
        await _authenticationService.RefreshTokenAsync(request);

        // Assert
        _tokenServiceMock.Verify(t => t.GenerateToken(user), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync__Should_StoreNewRefreshTokenAndRevokeOldToken_WhenRefreshTokenIsValid()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            UserId = 1,
            ExpiresAtUtc = currentTime.AddSeconds(1).UtcDateTime,
        };
        User user = new()
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = "hashedPassword"
        };
        var tokenResponse = ("jwtToken", currentTime.AddDays(1).UtcDateTime);
        var refreshTokenResponse = new RefreshTokenGeneratedDTO
        {
            RawToken = "rawRefreshToken",
            HashedToken = "hashedRefreshToken",
            ExpiresAtUtc = currentTime.AddDays(7).UtcDateTime
        };
        var newRefreshToken = new RefreshToken
        {
            TokenHash = refreshTokenResponse.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = refreshTokenResponse.ExpiresAtUtc,
            CreatedAtUtc = currentTime.UtcDateTime
        };

        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(tokenResponse);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(refreshTokenResponse);
        _unitOfWorkMock.Setup(u => u.RefreshTokens.UpdateAsync(It.IsAny<RefreshToken>()));
        _unitOfWorkMock.Setup(u => u.RefreshTokens.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _fakeTimeProvider.SetUtcNow(currentTime);

        // Act
        await _authenticationService.RefreshTokenAsync(request);

        // Assert
        _unitOfWorkMock.Verify(u => u.RefreshTokens.UpdateAsync(
            It.Is<RefreshToken>(rt =>
                rt.Id == storedToken.Id &&
                rt.UserId == storedToken.UserId &&
                rt.ExpiresAtUtc == storedToken.ExpiresAtUtc &&
                rt.RevokedAtUtc.HasValue &&
                rt.RevokedAtUtc == currentTime.UtcDateTime
            )
        ), Times.Once);
        _unitOfWorkMock.Verify(u => u.RefreshTokens.AddAsync(
            It.Is<RefreshToken>(rt =>
                rt.TokenHash == newRefreshToken.TokenHash &&
                rt.UserId == newRefreshToken.UserId &&
                rt.CreatedByIp == newRefreshToken.CreatedByIp &&
                rt.UserAgent == newRefreshToken.UserAgent &&
                rt.DeviceName == newRefreshToken.DeviceName &&
                rt.ExpiresAtUtc == newRefreshToken.ExpiresAtUtc &&
                rt.CreatedAtUtc == newRefreshToken.CreatedAtUtc
            )
        ), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [TestMethod]
    public async Task RefreshTokenAsync__Should_ReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        string refreshToken = "someRefreshToken";
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        RefreshTokenDetailRequest request = new(refreshToken, "127.0.0.1", "UnitTestAgent", "UnitTestDevice");
        RefreshToken storedToken = new()
        {
            UserId = 1,
            ExpiresAtUtc = currentTime.AddSeconds(1).UtcDateTime,
        };
        User user = new()
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = "hashedPassword",
        };
        var tokenResponse = ("jwtToken", currentTime.AddDays(1).UtcDateTime);
        var refreshTokenResponse = new RefreshTokenGeneratedDTO
        {
            RawToken = "rawRefreshToken",
            HashedToken = "hashedRefreshToken",
            ExpiresAtUtc = currentTime.AddDays(7).UtcDateTime
        };
        var newRefreshToken = new RefreshToken
        {
            TokenHash = refreshTokenResponse.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = refreshTokenResponse.ExpiresAtUtc,
            CreatedAtUtc = currentTime.UtcDateTime
        };

        _tokenServiceMock.Setup(t => t.ComputeHash(It.IsAny<string>())).Returns("hashedRefreshToken");
        _unitOfWorkMock.Setup(u => u.RefreshTokens.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync(storedToken);
        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(tokenResponse);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(refreshTokenResponse);

        _unitOfWorkMock.Setup(u => u.RefreshTokens.UpdateAsync(It.IsAny<RefreshToken>()));
        _unitOfWorkMock.Setup(u => u.RefreshTokens.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _fakeTimeProvider.SetUtcNow(currentTime);

        // Act
        var response = await _authenticationService.RefreshTokenAsync(request);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(tokenResponse.Item1, response.Token);
        Assert.AreEqual(tokenResponse.Item2, response.TokenExpiresIn);
        Assert.AreEqual(refreshTokenResponse.RawToken, response.RefreshToken);
        Assert.AreEqual(refreshTokenResponse.ExpiresAtUtc, response.RefreshTokenExpiresIn);
    }

    #endregion RefreshTokenAsync Tests

    #region RegisterAsync Tests

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task RegisterAsync__ShouldThrow_ArgumentException_WhenEmailIsNullOrEmpty(string email)
    {
        // Arrange
        var request = new AuthenticateUserRequest
        {
            Email = email,
            Password = "ValidPassword123!"
        };

        // Act
        Task act() => _authenticationService.RegisterAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(act);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task RegisterAsync__ShouldThrow_ArgumentException_WhenPasswordIsNullOrEmpty(string password)
    {
        // Arrange
        var request = new AuthenticateUserRequest
        {
            Email = "user@example.com",
            Password = password
        };

        // Act
        Task act() => _authenticationService.RegisterAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(act);
    }

    [TestMethod]
    public async Task RegisterAsync__ShouldThrow_InvalidOperationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new AuthenticateUserRequest
        {
            Email = "user@example.com",
            Password = "ValidPassword123!"
        };
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(new User());
        // Act
        Task act() => _authenticationService.RegisterAsync(request);

        // Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(act);
        _unitOfWorkMock.Verify(u => u.Users.GetByEmailAsync(request.Email.ToLower().Trim()), Times.Once);
    }

    [TestMethod]
    [DataRow("user@example.com")]
    [DataRow("USER2@ExaMplE.com    ")]
    [DataRow("   USER.Test@example.COM")]
    [DataRow("   User.Test@Example.Com    ")]
    public async Task RegisterAsync__Should_CreateANewUser_WhenEmailDoesNotExist(string email)
    {
        // Arrange
        var request = new AuthenticateUserRequest
        {
            Email = email,
            Password = "ValidPassword123!"
        };
        var currentTime = DateTimeOffset.UtcNow;
        string normalizedEmail = email.ToLower().Trim();
        string displayName = normalizedEmail.Split('@')[0];
        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns(("hashedPassword", new byte[] { 1, 2, 3, 4 }));
        _unitOfWorkMock.Setup(u => u.Users.AddAsync(It.IsAny<User>())).ReturnsAsync(new User());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _fakeTimeProvider.SetUtcNow(currentTime);

        // Act
        await _authenticationService.RegisterAsync(request);

        // Assert
        _unitOfWorkMock.Verify(u => u.Users.GetByEmailAsync(normalizedEmail), Times.Once);
        _passwordHasherMock.Verify(p => p.HashPassword(request.Password), Times.Once);
        _unitOfWorkMock.Verify(u => u.Users.AddAsync(It.Is<User>(user =>
            user.Email == normalizedEmail &&
            user.PasswordHash == "hashedPassword" &&
            user.Salt == Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }) &&
            user.CreatedAt == currentTime.UtcDateTime &&
            !user.EmailConfirmed &&
            user.DisplayName == displayName &&
            user.IsActive == true
        )), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion RegisterAsync Tests
}