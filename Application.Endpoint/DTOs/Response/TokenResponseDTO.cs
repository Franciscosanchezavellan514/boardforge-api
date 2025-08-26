namespace DevStack.Application.BoardForge.DTOs.Response;

public class TokenResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresIn { get; set; }
}
