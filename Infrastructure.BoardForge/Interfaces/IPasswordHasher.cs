namespace DevStack.Infrastructure.BoardForge.Interfaces;

public interface IPasswordHasher
{
    (string hashedPassword, byte[] salt) HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword, byte[] salt);
}
