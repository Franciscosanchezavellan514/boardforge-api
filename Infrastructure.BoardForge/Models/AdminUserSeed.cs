namespace DevStack.Infrastructure.BoardForge.Models;

public class AdminUserSeed
{
    public const string SectionName = "AdminUserSeed";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsValid => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}