using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.Endpoint.DTOs.Request;
using DevStack.Application.Endpoint.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Interfaces.Repositories;
using DevStack.Infrastructure.BoardForge.Interfaces;

namespace DevStack.Infrastructure.BoardForge.Services;

public class AuthenticationService(IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher) : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<TokenResponseDTO> AuthenticateAsync(AuthenticateUserRequest request)
    {
        string email = request.Email;
        string password = request.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password must be provided.");

        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null || !_passwordHasher.VerifyHashedPassword(password, user.PasswordHash, Convert.FromBase64String(user.Salt)))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        (string token, DateTime expiresAtUtc) = _tokenService.GenerateToken(user);
        RefreshTokenGeneratedDTO refreshTokenDto = _tokenService.GenerateRefreshToken();
        RefreshToken refreshToken = new()
        {
            TokenHash = refreshTokenDto.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = refreshTokenDto.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new TokenResponseDTO
        {
            Token = token,
            ExpiresIn = expiresAtUtc
        };
    }

    public async Task<UserResponse> RegisterAsync(AuthenticateUserRequest request)
    {
        string email = request.Email;
        string password = request.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password must be provided.");

        var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
        if (existingUser != null) throw new InvalidOperationException("A user with this email already exists.");

        (string hashedPassword, byte[] salt) = _passwordHasher.HashPassword(password);

        User newUser = new()
        {
            Email = email,
            PasswordHash = hashedPassword,
            DisplayName = email.Split('@')[0],
            Salt = Convert.ToBase64String(salt),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = false,
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        return new UserResponse
        {
            Id = newUser.Id,
            DisplayName = newUser.DisplayName,
            Email = newUser.Email,
            EmailConfirmed = newUser.EmailConfirmed,
            CreatedAt = newUser.CreatedAt,
        };
    }
}
