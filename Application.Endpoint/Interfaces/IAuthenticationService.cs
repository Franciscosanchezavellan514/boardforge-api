using DevStack.Application.BoardForge.DTOs.Response;

namespace DevStack.Application.BoardForge.Interfaces;

public interface IAuthenticationService
{
    Task<TokenResponseDTO> AuthenticateAsync(string email, string password);
}
