using DevStack.Application.Endpoint.DTOs.Request;

namespace DevStack.BoardForgeAPI.Utils;

public class AuthRequestBuilder
{
    private readonly HttpContext _httpContext;

    private AuthRequestBuilder(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public static AuthRequestBuilder Instance(HttpContext httpContext) => new(httpContext);

    public AuthenticateUserRequest BuildAuthenticateUserRequest(UserRequest userRequest)
    {
        if (userRequest == null)
        {
            throw new ArgumentNullException(nameof(userRequest), "User request cannot be null.");
        }

        string ipAddress = _httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        string userAgent = _httpContext.Request.Headers.UserAgent.ToString() ?? "Unknown";
        string deviceName = _httpContext.Request.Headers.ContainsKey("Device-Name")
            ? _httpContext.Request.Headers["Device-Name"].ToString()
            : "Unknown";

        return new AuthenticateUserRequest
        {
            Email = userRequest.Email,
            Password = userRequest.Password,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = deviceName
        };
    }

    public RefreshTokenDetailRequest BuildRefreshTokenDetailRequest(RefreshTokenRequest refreshTokenRequest)
    {
        string refreshToken = refreshTokenRequest.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token must be provided.");
        }

        string ipAddress = _httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        string userAgent = _httpContext.Request.Headers.UserAgent.ToString() ?? "Unknown";
        string deviceName = _httpContext.Request.Headers.ContainsKey("Device-Name")
            ? _httpContext.Request.Headers["Device-Name"].ToString()
            : "Unknown";

        return new RefreshTokenDetailRequest(
            RefreshToken: refreshToken,
            IpAddress: ipAddress,
            UserAgent: userAgent,
            DeviceName: deviceName
        );
    }
}
