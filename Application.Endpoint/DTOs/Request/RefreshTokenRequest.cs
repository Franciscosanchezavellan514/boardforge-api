namespace DevStack.Application.Endpoint.DTOs.Request;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
}
