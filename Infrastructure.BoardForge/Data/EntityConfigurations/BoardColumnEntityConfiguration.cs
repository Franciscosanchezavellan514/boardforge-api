using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class BoardColumnEntityConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
        builder.HasOne(e => e.Board)
            .WithMany(b => b.Columns)
            .HasForeignKey(e => e.BoardId);
        builder.HasMany(e => e.Cards)
            .WithOne(c => c.BoardColumn)
            .HasForeignKey(c => c.BoardColumnId);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}