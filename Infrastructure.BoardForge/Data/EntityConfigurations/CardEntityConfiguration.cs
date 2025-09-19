using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class CardEntityConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Board)
            .WithMany(b => b.Cards)
            .HasForeignKey(e => e.BoardId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.BoardColumn)
            .WithMany(c => c.Cards)
            .HasForeignKey(e => e.BoardColumnId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Card)
            .HasForeignKey(a => a.CardId);
        builder.HasMany(e => e.Comments)
            .WithOne(c => c.Card)
            .HasForeignKey(c => c.CardId);
        builder.HasMany(e => e.Labels)
            .WithOne(cl => cl.Card)
            .HasForeignKey(cl => cl.CardId);
        builder.HasOne(e => e.Owner)
            .WithMany(u => u.AssignedCards)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.Team)
            .WithMany(t => t.Cards)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}