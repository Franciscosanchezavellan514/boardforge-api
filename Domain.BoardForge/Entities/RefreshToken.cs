namespace DevStack.Domain.BoardForge.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
}
