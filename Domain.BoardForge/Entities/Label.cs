using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class Label : AuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    /// <summary>
    /// Hex (#RRGGBB) or named color
    /// </summary>
    [MaxLength(32)]
    public string ColorHex { get; set; } = "#FFFFFF"; // Default to white
    public ICollection<CardLabel> CardLabels { get; set; } = new HashSet<CardLabel>();
}
