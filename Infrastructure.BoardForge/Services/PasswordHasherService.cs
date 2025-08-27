using System.Security.Cryptography;
using DevStack.Infrastructure.BoardForge.Interfaces;

namespace DevStack.Infrastructure.BoardForge.Services;

public class PasswordHasherService : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int HashByteSize = 32; // bytes - 256 bits
    private const int SaltByteSize = 16; // bytes - 128 bits

    public (string hashedPassword, byte[] salt) HashPassword(string password)
    {
        byte[] salt = GenerateSalt();
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(HashByteSize);
        return (Convert.ToBase64String(hash), salt);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword, byte[] salt)
    {
        byte[] storedHash = Convert.FromBase64String(hashedPassword);
        using var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, Iterations, HashAlgorithmName.SHA256);
        byte[] computedHash = pbkdf2.GetBytes(HashByteSize);
        // Avoid/prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltByteSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
