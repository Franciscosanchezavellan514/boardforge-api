using System.Security.Cryptography;
using System.Text;
using DevStack.Domain.BoardForge.Interfaces.Services;

namespace DevStack.Infrastructure.BoardForge.Services;

public class StringUtilsService : IStringUtilsService
{
    public string GetColorFromChar(char c)
    {
        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes([c]));

        return $"#{hash[0]:X2}{hash[1]:x2}{hash[2]:X2}";
    }

    public string Normalize(string str)
    {
        return str.ToLower().Trim();
    }

    public string NormalizeAndReplaceWhitespaces(string str, char c)
    {
        return str.ToLower().Trim().Replace(' ', c);
    }
}