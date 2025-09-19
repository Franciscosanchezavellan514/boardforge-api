using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class CardCommentEntityConfiguration : IEntityTypeConfiguration<CardComment>
{
    public void Configure(EntityTypeBuilder<CardComment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Card)
            .WithMany(c => c.Comments)
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}