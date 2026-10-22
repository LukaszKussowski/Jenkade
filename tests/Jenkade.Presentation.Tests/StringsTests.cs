using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Jenkade.Presentation.Tests;

public class StringsTests
{
    [Fact]
    public void Every_string_has_an_English_and_a_Polish_value()
    {
        var english = Values(CultureInfo.InvariantCulture);
        var polish = Values(CultureInfo.GetCultureInfo("pl"));

        Assert.NotEmpty(english);
        Assert.Equal(english.Keys.Order(), polish.Keys.Order());
        Assert.All(english.Concat(polish), entry => Assert.False(string.IsNullOrWhiteSpace(entry.Value), entry.Key));
    }

    [Fact]
    public void No_string_uses_a_term_the_glossary_avoids()
    {
        var avoided = AvoidedTerms();
        Assert.Contains("server", avoided);
        Assert.Contains("zadanie", avoided);

        var offending =
            from entry in Values(CultureInfo.InvariantCulture).Concat(Values(CultureInfo.GetCultureInfo("pl")))
            from term in avoided
            where Regex.IsMatch(entry.Value, $@"\b{Regex.Escape(term)}\b", RegexOptions.IgnoreCase)
            select $"{entry.Key}: \"{entry.Value}\" uses \"{term}\"";
        Assert.Empty(offending);
    }

    // Terms from "_Avoid_: a, b (note)" lines and the "(not „a”, „b”)" part of "_Polish UI_" lines.
    private static string[] AvoidedTerms()
    {
        var glossary = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "GLOSSARY.md"));
        var english = glossary
            .Where(line => line.StartsWith("_Avoid_:", StringComparison.Ordinal))
            .SelectMany(line => line["_Avoid_:".Length..].Split(','))
            .Select(term => Regex.Replace(term, @"\(.*?\)", "").Trim());
        var polish = glossary
            .Where(line => line.StartsWith("_Polish UI_:", StringComparison.Ordinal))
            .SelectMany(line => Regex.Matches(line, @"\(not (.*?)\)").SelectMany(not => Regex.Matches(not.Groups[1].Value, "\u201E(.*?)\u201D")))
            .Select(quoted => quoted.Groups[1].Value);
        return [.. english.Concat(polish).Where(term => term.Length > 0).Distinct()];
    }

    // Only the resources compiled for exactly this culture, without falling back to the neutral English ones.
    private static Dictionary<string, string> Values(CultureInfo culture) =>
        Strings.ResourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)!
            .Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
}
