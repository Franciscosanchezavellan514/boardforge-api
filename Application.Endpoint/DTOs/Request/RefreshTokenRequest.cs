namespace DevStack.Application.Endpoint.DTOs.Request;

public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenDetailRequest(
    string RefreshToken,
    string IpAddress,
    string UserAgent,
    string DeviceName
);