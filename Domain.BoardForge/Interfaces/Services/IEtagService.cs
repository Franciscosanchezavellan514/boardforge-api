namespace DevStack.Domain.BoardForge.Interfaces.Services;

public interface IEtagService
{
    /// <summary>
    /// Creates an ETag from a row version byte array.<br />
    /// Weak ETag format: W/"<base64-encoded-row-version>" <br />
    /// Strong ETag format: "<base64-encoded-row-version>"
    /// </summary>
    /// <param name="rowVersion"></param>
    /// <returns>The ETag string.</returns>
    string FromRowVersion(byte[] rowVersion, bool isWeak = true);
    /// <summary>
    /// Tries to parse the If-Match header to extract the row version byte array.
    /// </summary>
    /// <param name="ifMatchHeader">The If-Match header value.</param>
    /// <param name="rowVersion">The extracted row version byte array if parsing is successful; otherwise, null.</param>
    /// <returns>True if parsing was successful; otherwise, false.</returns>
    bool TryParseIfMatch(string? ifMatchHeader, out byte[] rowVersion, out bool isWeak);
}