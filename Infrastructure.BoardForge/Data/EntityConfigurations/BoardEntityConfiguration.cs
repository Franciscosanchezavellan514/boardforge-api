using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class BoardEntityConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
        builder.HasMany(e => e.Columns)
            .WithOne(c => c.Board)
            .HasForeignKey(c => c.BoardId);
        builder.HasMany(e => e.Cards)
            .WithOne(c => c.Board)
            .HasForeignKey(c => c.BoardId);
        builder.HasOne(e => e.Team)
            .WithMany(t => t.Boards)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}