using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.Services;
using DevStack.Domain.BoardForge.Entities;
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
        AuthenticateAsyncSetupHappyPath(request, out User user, out RefreshToken expectedRefreshToken);
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
        request = CreateAuthenticateUserRequest();
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
}