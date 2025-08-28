using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class CardAttachment : BaseEntity, ITeamResource
{
    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    [Required, MaxLength(2083)] // Max URL length in IE
    public string FileUrl { get; set; } = string.Empty;
    [MaxLength(255)]
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    [Required]
    public int CardId { get; set; }
    public Card? Card { get; set; }

    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    [Required]
    public int UploadedById { get; set; }
    public User? UploadedBy { get; set; }

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
