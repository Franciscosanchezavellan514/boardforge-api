using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
        // Ensure email is unique to prevent duplicate user accounts and support user identification.
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}