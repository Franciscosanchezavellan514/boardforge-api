namespace DevStack.Application.BoardForge.DTOs.Request;

public class AuthenticateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}
