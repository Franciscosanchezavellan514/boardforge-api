using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class CardAttachmentEntityConfiguration : IEntityTypeConfiguration<CardAttachment>
{
    public void Configure(EntityTypeBuilder<CardAttachment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Card)
            .WithMany(c => c.Attachments)
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(e => e.UploadedBy)
            .WithMany(u => u.Attachments)
            .HasForeignKey(e => e.UploadedById)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}