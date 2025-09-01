using DevStack.Domain.BoardForge.Entities;
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
            // Ensure email is unique to prevent duplicate user accounts and support user identification.
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(e => e.TokenHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CreatedByIp).HasMaxLength(64);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.Property(e => e.DeviceName).HasMaxLength(128);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
            entity.HasMany(e => e.Columns)
                  .WithOne(c => c.Board)
                  .HasForeignKey(c => c.BoardId);
            entity.HasMany(e => e.Cards)
                  .WithOne(c => c.Board)
                  .HasForeignKey(c => c.BoardId);
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Boards)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<BoardColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn(seed: 1000, increment: 1);
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.Columns)
                  .HasForeignKey(e => e.BoardId);
            entity.HasMany(e => e.Cards)
                  .WithOne(c => c.BoardColumn)
                  .HasForeignKey(c => c.BoardColumnId);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Board)
                  .WithMany(b => b.Cards)
                  .HasForeignKey(e => e.BoardId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.BoardColumn)
                  .WithMany(c => c.Cards)
                  .HasForeignKey(e => e.BoardColumnId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Attachments)
                  .WithOne(a => a.Card)
                  .HasForeignKey(a => a.CardId);
            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.Card)
                  .HasForeignKey(c => c.CardId);
            entity.HasMany(e => e.Labels)
                  .WithOne(cl => cl.Card)
                  .HasForeignKey(cl => cl.CardId);
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.AssignedCards)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Cards)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CardAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Card)
                  .WithMany(c => c.Attachments)
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.UploadedBy)
                  .WithMany(u => u.Attachments)
                  .HasForeignKey(e => e.UploadedById)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CardComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Card)
                  .WithMany(c => c.Comments)
                  .HasForeignKey(e => e.CardId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Author)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Label>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.CardLabels)
                  .WithOne(cl => cl.Label)
                  .HasForeignKey(cl => cl.LabelId);
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Labels)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

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
