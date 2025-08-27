using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.Endpoint.DTOs.Request;

namespace DevStack.Application.BoardForge.Interfaces;

public interface IAuthenticationService
{
    Task<TokenResponseDTO> AuthenticateAsync(AuthenticateUserRequest request);
    Task<UserResponse> RegisterAsync(AuthenticateUserRequest request);
    Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenDetailRequest request);
}
