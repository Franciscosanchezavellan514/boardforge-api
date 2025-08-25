using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Data;

public class BoardForgeDbContext : DbContext
{
    public BoardForgeDbContext(DbContextOptions<BoardForgeDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
