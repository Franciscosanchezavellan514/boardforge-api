using System.ComponentModel.DataAnnotations;

namespace DevStack.Domain.BoardForge.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    [Required]
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}

public abstract class SoftDeletableEntity : AuditableEntity
{
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}

public abstract class VersionedEntity : SoftDeletableEntity
{
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}