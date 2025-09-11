using DevStack.Application.BoardForge.Interfaces;
using DevStack.Domain.BoardForge.Entities;
using DevStack.Infrastructure.BoardForge.Data;
using DevStack.Infrastructure.BoardForge.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

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
    public async Task GetUserRoleAsync__ShouldReturnRole_WhenMembershipExists()
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

        _memoryCacheMock.SetupTryGetValueMiss(cacheKey);
        _memoryCacheMock.SetupSet(cacheKey, It.Ref<TeamMembershipRole.Role>.IsAny, It.IsAny<TimeSpan>());

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual(TeamMembershipRole.Role.Owner, role);
    }

    [TestMethod]
    public async Task GetUserRoleAsync__Should_SetCache_WhenRoleIsNotNull()
    {
        // Arrange
        int userId = 2;
        int teamId = 2;
        string cacheKey = $"TeamRole.{userId}.{teamId}";
        var membership = new TeamMembership
        {
            UserId = userId,
            TeamId = teamId,
            Role = TeamMembershipRole.FromEnum(TeamMembershipRole.Role.Viewer),
            IsActive = true
        };
        _dbContext.TeamMemberships.Add(membership);
        _dbContext.SaveChanges();

        _memoryCacheMock.SetupTryGetValueMiss(cacheKey);
        var cacheEntry = _memoryCacheMock.SetupSet(cacheKey, It.IsAny<TeamMembershipRole.Role>(), It.IsAny<TimeSpan>());

        // Act
        await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        _memoryCacheMock.Verify(mc => mc.CreateEntry(cacheKey), Times.Once());
        Assert.AreEqual(TeamMembershipRole.Role.Viewer, cacheEntry.Object.Value);
        Assert.AreEqual(TimeSpan.FromMinutes(10), cacheEntry.Object.AbsoluteExpirationRelativeToNow);
    }

    [TestMethod]
    public async Task GetUserRoleAsync__Should_ReturnNull_WhenMembershipDoesNotExist()
    {
        // Arrange
        int userId = 3;
        int teamId = 3;
        string cacheKey = $"TeamRole.{userId}.{teamId}";

        _memoryCacheMock.SetupAllProperties();
        _memoryCacheMock.SetupTryGetValueMiss(cacheKey);

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNull(role);
    }

    [TestMethod]
    public async Task GetUserRoleAsync__Should_NotSetCache_WhenMembershipDoesNotExist()
    {
        // Arrange
        int userId = 4;
        int teamId = 4;
        string cacheKey = $"TeamRole.{userId}.{teamId}";

        _memoryCacheMock.SetupAllProperties();
        _memoryCacheMock.SetupTryGetValueMiss(cacheKey);

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNull(role);
        _memoryCacheMock.Verify(mc => mc.CreateEntry(cacheKey), Times.Never);
    }

    [TestMethod]
    public async Task GetUserRoleAsync__Should_ReturnsRoleFromCache_WhenCacheHit()
    {
        // Arrange
        int userId = 5;
        int teamId = 5;
        string cacheKey = $"TeamRole.{userId}.{teamId}";
        var cachedRole = TeamMembershipRole.Role.Member;

        _memoryCacheMock.SetupAllProperties();
        _memoryCacheMock.SetupTryGetValueHit(cacheKey, cachedRole);

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual(cachedRole, role);
    }

    [TestMethod]
    public async Task GetUserRoleAsync__Should_NotSetCache_WhenCacheHit()
    {
        // Arrange
        int userId = 6;
        int teamId = 6;
        string cacheKey = $"TeamRole.{userId}.{teamId}";
        var cachedRole = TeamMembershipRole.Role.Owner;

        _memoryCacheMock.SetupAllProperties();
        _memoryCacheMock.SetupTryGetValueHit(cacheKey, cachedRole);

        // Act
        var role = await _service.GetUserRoleAsync(userId, teamId);

        // Assert
        Assert.IsNotNull(role);
        _memoryCacheMock.Verify(mc => mc.CreateEntry(cacheKey), Times.Never);
    }
}
