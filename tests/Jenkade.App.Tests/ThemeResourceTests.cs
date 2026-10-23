using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;

namespace Jenkade.App.Tests;

public class ThemeResourceTests
{
    // A missing DynamicResource key fails silently: no build error, no binding error, just default colours.
    // Fluent keys from WinUI (e.g. TextFillColorSecondaryBrush) don't all exist in Avalonia's Fluent theme.
    [AvaloniaFact]
    public void Every_resource_the_views_use_exists_in_the_light_and_dark_theme()
    {
        var keys = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "axaml"), "*.axaml", SearchOption.AllDirectories)
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"\{DynamicResource (\w+)\}"), (file, match) => (File: Path.GetFileName(file), Key: match.Groups[1].Value))
            .ToList();
        Assert.NotEmpty(keys);

        var missing =
            from use in keys
            from theme in new[] { ThemeVariant.Light, ThemeVariant.Dark }
            where !Application.Current!.TryGetResource(use.Key, theme, out _)
            select $"{use.File}: {use.Key} ({theme})";
        Assert.Empty(missing);
    }
}
