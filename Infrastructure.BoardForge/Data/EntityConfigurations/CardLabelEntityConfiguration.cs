using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class CardLabelEntityConfiguration : IEntityTypeConfiguration<CardLabel>
{
    public void Configure(EntityTypeBuilder<CardLabel> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.CardId, e.LabelId }).IsUnique();
        builder.HasOne(e => e.Card)
            .WithMany(c => c.Labels)
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Label)
            .WithMany(l => l.CardLabels)
            .HasForeignKey(e => e.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}