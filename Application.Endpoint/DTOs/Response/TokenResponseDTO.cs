namespace DevStack.Application.BoardForge.DTOs.Response;

public class TokenResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresIn { get; set; }
    public DateTime RefreshTokenExpiresIn { get; set; }
}
