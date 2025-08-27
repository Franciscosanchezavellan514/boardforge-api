using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
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

    public async Task<TokenResponseDTO> AuthenticateAsync(string email, string password)
    {
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
            CreatedByIp = "127.0.0.1", // This should be replaced with the actual IP address
            UserAgent = "Unknown", // This should be replaced with the actual user agent
            DeviceName = "Unknown", // This should be replaced with the actual device name
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
}
