using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class LabelEntityConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasMany(e => e.CardLabels)
            .WithOne(cl => cl.Label)
            .HasForeignKey(cl => cl.LabelId);
        builder.HasOne(e => e.Team)
            .WithMany(t => t.Labels)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.HasIndex(e => new { e.TeamId, e.NormalizedName }).IsUnique();
    }
}