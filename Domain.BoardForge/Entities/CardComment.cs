using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class CardComment : BaseEntity
{
    [Required]
    public int CardId { get; set; }
    public Card? Card { get; set; }

    [Required]
    public int AuthorId { get; set; }
    public User? Author { get; set; }

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
