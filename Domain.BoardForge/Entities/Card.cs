using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class Card : VersionedEntity, ITeamResource
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public int Order { get; set; }

    public int? BoardColumnId { get; set; }
    public BoardColumn? BoardColumn { get; set; }

    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public int? BoardId { get; set; }
    public Board? Board { get; set; }

    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    // Navigation properties
    public ICollection<CardLabel> Labels { get; set; } = new HashSet<CardLabel>();
    public ICollection<CardComment> Comments { get; set; } = new HashSet<CardComment>();
    public ICollection<CardAttachment> Attachments { get; set; } = new HashSet<CardAttachment>();
}
