namespace DevStack.Domain.BoardForge.Interfaces.Services;

public interface IStringUtilsService
{
    string GetColorFromChar(char c);
    string Normalize(string str);
    string NormalizeAndReplaceWhitespaces(string str, char c);
}