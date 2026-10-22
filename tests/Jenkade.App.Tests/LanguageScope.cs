using System.Globalization;

namespace Jenkade.App.Tests;

/// <summary>
/// Switches the UI language for one test and restores it afterwards. Headless tests share the dispatcher thread,
/// so a language left behind would leak into the next test.
/// </summary>
public sealed class LanguageScope : IDisposable
{
    private readonly CultureInfo culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo uiCulture = CultureInfo.CurrentUICulture;

    public LanguageScope(string language) =>
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);

    public void Dispose()
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }
}
