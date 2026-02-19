namespace LicenseNexus.API.Helpers;

public class SearchTokenizer
{
    public static IEnumerable<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) 
            return Enumerable.Empty<string>();

        return text.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '-', '_', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Distinct();
    }
}