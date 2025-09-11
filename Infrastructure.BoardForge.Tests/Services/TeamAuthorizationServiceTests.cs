using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Data;
using DevStack.Infrastructure.BoardForge.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json.Linq;

namespace DevStack.Infrastructure.BoardForge.Tests.Services;

[TestClass]
public class TeamAuthorizationServiceTests : IDisposable
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly BoardForgeDbContext _dbContext;
    private readonly ITeamAuthorizationService _service;

    public TeamAuthorizationServiceTests()
    {
        _dbContext = DBContextHelper.CreateContext();
        _memoryCacheMock = new Mock<IMemoryCache>();
        _service = new TeamAuthorizationService(_dbContext, _memoryCacheMock.Object);
    }

    /// <summary>
    /// Dispose the test class and release resources.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCacheMock.Reset();
        GC.SuppressFinalize(this);
    }

    [TestInitialize]
    public void Setup()
    {
        // Any setup before each test can be done here.
        //_memoryCacheMock.SetupAllProperties();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Any cleanup after each test can be done here.
        _memoryCacheMock.Reset();
    }

    [TestMethod]
    public async Task GetUserRoleAsync__ReturnsRole_WhenMembershipExists()
    {
        // Arrange
        int userId = 1;
        int teamId = 1;
        string cacheKey = $"TeamRole.{userId}.{teamId}";
        var membership = new TeamMembership
        {
            UserId = userId,
            TeamId = teamId,
            Role = TeamMembershipRole.FromEnum(TeamMembershipRole.Role.Owner),
            IsActive = true
        };
        _dbContext.TeamMemberships.Add(membership);
        await _dbContext.SaveChangesAsync();

        _memoryCacheMock.SetupAllProperties();
        _memoryCacheMock.SetupTryGetValueMiss(cacheKey);
        _memoryCacheMock.SetupSet(cacheKey, TeamMembershipRole.Role.Owner, TimeSpan.FromMinutes(10));

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual(TeamMembershipRole.Role.Owner, role);
    }
}
