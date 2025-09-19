using DevStack.Domain.BoardForge.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;

public class TeamEntityConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
        builder.HasMany(e => e.Boards)
            .WithOne(b => b.Team)
            .HasForeignKey(b => b.TeamId);
        builder.HasMany(e => e.TeamMemberships)
            .WithOne(tm => tm.Team)
            .HasForeignKey(tm => tm.TeamId);
        builder.HasMany(e => e.Labels)
            .WithOne(l => l.Team)
            .HasForeignKey(l => l.TeamId);
        builder.HasMany(e => e.Cards)
            .WithOne(c => c.Team)
            .HasForeignKey(e => e.TeamId);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
    }
}