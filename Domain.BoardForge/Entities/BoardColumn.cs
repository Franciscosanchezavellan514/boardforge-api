using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class BoardColumn : BaseEntity, ITeamResource
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public int Position { get; set; }

    [Required]
    public int BoardId { get; set; }
    public Board? Board { get; set; }

    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public DateTime CreateAt { get; set; }
    public int CreatedBy { get; set; }

    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
