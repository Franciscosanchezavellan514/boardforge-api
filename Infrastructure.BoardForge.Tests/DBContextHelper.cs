using System.Diagnostics.CodeAnalysis;
using DevStack.Infrastructure.BoardForge.Data;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Infrastructure.BoardForge.Tests;

[ExcludeFromCodeCoverage]
public static class DBContextHelper
{
    public static BoardForgeDbContext CreateContext(Action<BoardForgeDbContext>? setup, string databaseName = "BoardForgeDBTest")
    {
        var options = new DbContextOptionsBuilder<BoardForgeDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        if (setup is not null)
        {
            var context = new BoardForgeDbContext(options);
            setup(context);
            context.SaveChanges();
        }

        return new BoardForgeDbContext(options);
    }

    public static BoardForgeDbContext CreateContext()
    {
        return CreateContext(null);
    }

    public static void ClearDatabase(BoardForgeDbContext context)
    {
        context.Database.EnsureDeleted();
        context.SaveChanges();
    }
}
