using System.Security.Claims;
using DevStack.BoardForgeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevStack.BoardForgeAPI.Controllers;

public class BaseApiController : ControllerBase
{
    // Gets the current user's ID, or throws an exception if not found
    protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ArgumentNullException("User ID not found"));
    // Gets the current user information from claims principal
    protected CurrentUser CurrentUser => CurrentUser.FromClaims(User);
}
