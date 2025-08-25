using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class BoardColumn : AuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public int Position { get; set; }

    [Required]
    public int BoardId { get; set; }
    public Board? Board { get; set; }

    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
