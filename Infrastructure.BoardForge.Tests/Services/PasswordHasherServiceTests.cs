using DevStack.Application.BoardForge.Interfaces;
using DevStack.Infrastructure.BoardForge.Services;

namespace DevStack.Infrastructure.BoardForge.Tests.Services;

[TestClass]
public class PasswordHasherServiceTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherServiceTests()
    {
        _passwordHasher = new PasswordHasherService();
    }

    [TestMethod]
    public void HashPassword__Should_Return_HashedPassword_And_Salt()
    {
        // Arrange
        string password = "TestPassword123!";

        // Act
        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);

        // Assert
        Assert.IsNotNull(hashedPassword);
        Assert.IsNotNull(salt);
        Assert.AreEqual(32, Convert.FromBase64String(hashedPassword).Length); // 256 bits
        Assert.AreEqual(16, salt.Length); // 128 bits
    }

    [TestMethod]
    public void VerifyHashedPassword__Should_Return_True()
    {
        // Arrange
        string password = "TestPassword123!";
        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);

        // Act
        bool isVerified = _passwordHasher.VerifyHashedPassword(hashedPassword, password, salt);

        // Assert
        Assert.IsTrue(isVerified);
    }

    [TestMethod]
    public void VerifyHashedPassword__Should_Return_False()
    {
        // Arrange
        string password = "TestPassword123!";
        var (hashedPassword, salt) = _passwordHasher.HashPassword(password);
        string wrongPassword = "WrongPassword456!";

        // Act
        bool isVerified = _passwordHasher.VerifyHashedPassword(hashedPassword, wrongPassword, salt);

        // Assert
        Assert.IsFalse(isVerified);
    }
}
