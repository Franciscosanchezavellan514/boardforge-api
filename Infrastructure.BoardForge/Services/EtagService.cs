using DevStack.Domain.BoardForge.Interfaces.Services;

namespace DevStack.Infrastructure.BoardForge.Services;

public class EtagService : IEtagService
{
    public string FromRowVersion(byte[] rowVersion, bool weak = true)
    {
        var base64RowVersion = Convert.ToBase64String(rowVersion);
        return weak ? $"W/\"{base64RowVersion}\"" : $"\"{base64RowVersion}\"";
    }

    public bool TryParseIfMatch(string? ifMatchHeader, out byte[] rowVersion, out bool isWeak)
    {
        rowVersion = Array.Empty<byte>();
        isWeak = false;
        if (string.IsNullOrWhiteSpace(ifMatchHeader)) return false;

        var trimmed = ifMatchHeader.Trim();
        if (trimmed.StartsWith("W/"))
        {
            isWeak = true;
            // Remove the weak prefix
            trimmed = trimmed[2..].Trim();
        }

        // Must be quoted: "base64"
        if (trimmed.Length < 2 || trimmed[0] != '"' || trimmed[^1] != '"')
            return false;

        // Extract the base64 part without quotes
        var base64RowVersion = trimmed[1..^1];
        try
        {
            var rowVersionBytes = Convert.FromBase64String(base64RowVersion);
            rowVersion = rowVersionBytes;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}