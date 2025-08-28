using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class Board : SoftDeletableEntity, ITeamResource
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<BoardColumn> Columns { get; set; } = [];
    public ICollection<Card> Cards { get; set; } = [];
}
