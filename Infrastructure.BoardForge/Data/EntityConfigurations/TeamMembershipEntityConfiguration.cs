using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class TeamMembershipEntityConfiguration : IEntityTypeConfiguration<TeamMembership>
{
    public void Configure(EntityTypeBuilder<TeamMembership> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
        builder.HasOne(e => e.Team)
            .WithMany(t => t.TeamMemberships)
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}