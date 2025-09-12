using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.Services;
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
}