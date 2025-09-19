using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Data.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data;

public class BoardForgeDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardColumn> BoardColumns { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<CardAttachment> CardAttachments { get; set; }
    public DbSet<CardComment> CardComments { get; set; }
    public DbSet<CardLabel> CardLabels { get; set; }
    public DbSet<Label> Labels { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMembership> TeamMemberships { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public BoardForgeDbContext(DbContextOptions<BoardForgeDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BoardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BoardColumnEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CardEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CardAttachmentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CardCommentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CardEntityConfiguration());

        modelBuilder.Entity<CardLabel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CardId, e.LabelId }).IsUnique();
            entity.HasOne(e => e.Card)
                  .WithMany(c => c.Labels)
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Label)
                  .WithMany(l => l.CardLabels)
                  .HasForeignKey(e => e.LabelId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
            entity.HasMany(e => e.Boards)
                  .WithOne(b => b.Team)
                  .HasForeignKey(b => b.TeamId);
            entity.HasMany(e => e.TeamMemberships)
                  .WithOne(tm => tm.Team)
                  .HasForeignKey(tm => tm.TeamId);
            entity.HasMany(e => e.Labels)
                  .WithOne(l => l.Team)
                  .HasForeignKey(l => l.TeamId);
            entity.HasMany(e => e.Cards)
                  .WithOne(c => c.Team)
                  .HasForeignKey(e => e.TeamId);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TeamMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.TeamMemberships)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TeamMemberships)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
