using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;

namespace DevStack.Infrastructure.BoardForge.Services;

public class AuthenticationService : IAuthenticationService
{
    public Task<TokenResponseDTO> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password must be provided.");

        throw new NotImplementedException();
    }
}
