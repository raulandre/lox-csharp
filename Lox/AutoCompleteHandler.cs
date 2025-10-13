namespace Lox;

public class AutoCompleteHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' ', '.'];

    public string[] GetSuggestions(string text, int index)
    {
        var normalizedText = text.Trim().ToLowerInvariant();
        return [.. Scanner.Keywords
            .Select(keyword => keyword.Key)
            .Where(key => key.StartsWith(normalizedText))];
    }
}