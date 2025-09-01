using System.Security.Claims;

namespace DevStack.BoardForgeAPI.Models;

public class CurrentUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public static CurrentUser FromClaims(ClaimsPrincipal claimsPrincipal)
    {
        return new CurrentUser
        {
            Id = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new ArgumentNullException("User ID not found")),
            Email = claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            DisplayName = claimsPrincipal.FindFirstValue(ClaimTypes.Name) ?? string.Empty
        };
    }
}