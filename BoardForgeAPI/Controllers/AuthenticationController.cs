using System.Net.Mime;
using DevStack.Application.BoardForge.DTOs.Response;
using DevStack.Application.BoardForge.Interfaces;
using DevStack.Application.BoardForge.DTOs.Request;
using DevStack.BoardForgeAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DevStack.BoardForgeAPI.Models;

namespace DevStack.BoardForgeAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class AuthenticationController : BaseApiController
{
    private readonly IAuthenticationService _authService;
    private readonly IUsersService _usersService;

    public AuthenticationController(IAuthenticationService authService, IUsersService usersService)
    {
        _authService = authService;
        _usersService = usersService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDTO))]
    public async Task<IActionResult> Login([FromBody] UserRequest request)
    {
        var authRequest = AuthRequestBuilder.Instance(HttpContext).BuildAuthenticateUserRequest(request);
        var result = await _authService.AuthenticateAsync(authRequest);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponseDTO))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshRequest = AuthRequestBuilder.Instance(HttpContext).BuildRefreshTokenDetailRequest(request);
        var result = await _authService.RefreshTokenAsync(refreshRequest);
        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
    public async Task<IActionResult> Register([FromBody] UserRequest request)
    {
        var authRequest = AuthRequestBuilder.Instance(HttpContext).BuildAuthenticateUserRequest(request);
        var result = await _authService.RegisterAsync(authRequest);
        return Created(string.Empty, result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(HttpErrorResponse))]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _usersService.GetUserByIdAsync(CurrentUserId);
        return Ok(result);
    }
}
