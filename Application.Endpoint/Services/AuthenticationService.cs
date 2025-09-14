using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Domain.BoardForge.Exceptions;
using DevStack.Domain.BoardForge.Interfaces.Repositories;

namespace DevStack.Application.BoardForge.Services;

public class AuthenticationService(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IAuthenticationService
{
    public async Task<TokenResponseDTO> AuthenticateAsync(AuthenticateUserRequest request)
    {
        string email = request.Email;
        string password = request.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password must be provided.");

        var user = await unitOfWork.Users.GetByEmailAsync(email.ToLower().Trim());
        if (user == null || !passwordHasher.VerifyHashedPassword(user.PasswordHash, password, Convert.FromBase64String(user.Salt)))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        (string token, DateTime expiresAtUtc) = tokenService.GenerateToken(user);
        RefreshTokenGeneratedDTO refreshTokenDto = tokenService.GenerateRefreshToken();
        RefreshToken refreshToken = new()
        {
            TokenHash = refreshTokenDto.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = refreshTokenDto.ExpiresAtUtc,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        };

        await unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await unitOfWork.SaveChangesAsync();

        return new TokenResponseDTO
        {
            Token = token,
            TokenExpiresIn = expiresAtUtc,
            RefreshToken = refreshTokenDto.RawToken,
            RefreshTokenExpiresIn = refreshTokenDto.ExpiresAtUtc
        };
    }

    public async Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenDetailRequest request)
    {
        var hashedToken = tokenService.ComputeHash(request.RefreshToken);
        var existingToken = await unitOfWork.RefreshTokens.GetByTokenAsync(hashedToken);
        if (existingToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (existingToken.IsRevoked || existingToken.ExpiresAtUtc <= timeProvider.GetUtcNow().UtcDateTime)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        var user = await unitOfWork.Users.GetByIdAsync(existingToken.UserId);
        if (user == null)
        {
            throw new EntityNotFoundException("User not found.");
        }

        (string token, DateTime expiresAtUtc) = tokenService.GenerateToken(user);
        RefreshTokenGeneratedDTO newRefreshTokenDto = tokenService.GenerateRefreshToken();
        RefreshToken newRefreshToken = new()
        {
            TokenHash = newRefreshTokenDto.HashedToken,
            UserId = user.Id,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent,
            DeviceName = request.DeviceName,
            ExpiresAtUtc = newRefreshTokenDto.ExpiresAtUtc,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        };

        existingToken.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        await unitOfWork.RefreshTokens.UpdateAsync(existingToken);
        await unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await unitOfWork.SaveChangesAsync();

        return new TokenResponseDTO
        {
            Token = token,
            TokenExpiresIn = expiresAtUtc,
            RefreshToken = newRefreshTokenDto.RawToken,
            RefreshTokenExpiresIn = newRefreshTokenDto.ExpiresAtUtc
        };
    }

    public async Task<UserResponse> RegisterAsync(AuthenticateUserRequest request)
    {
        string email = request.Email;
        string password = request.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password must be provided.");

        email = email.ToLower().Trim();
        var existingUser = await unitOfWork.Users.GetByEmailAsync(email);
        if (existingUser != null) throw new InvalidOperationException("A user with this email already exists.");

        (string hashedPassword, byte[] salt) = passwordHasher.HashPassword(password);

        User newUser = new()
        {
            Email = email,
            PasswordHash = hashedPassword,
            DisplayName = email.Split('@')[0],
            Salt = Convert.ToBase64String(salt),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            IsActive = true,
            EmailConfirmed = false,
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.SaveChangesAsync();

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
