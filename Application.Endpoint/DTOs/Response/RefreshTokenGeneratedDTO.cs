namespace DevStack.Application.BoardForge.DTOs.Response;

public class RefreshTokenGeneratedDTO
{
    public string RawToken { get; set; } = string.Empty;
    public string HashedToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
