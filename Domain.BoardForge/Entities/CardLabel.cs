using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public class CardLabel : SoftDeletableEntity
{
    [Required]
    public int CardId { get; set; }
    public Card? Card { get; set; }

    [Required]
    public int LabelId { get; set; }
    public Label? Label { get; set; }
}