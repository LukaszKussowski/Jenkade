using System.Globalization;

namespace Jenkade.Presentation;

/// <summary>The language of Jenkade's UI text; dates and times keep the Windows regional format (ADR 0004).</summary>
public static class UiLanguage
{
    public static CultureInfo English { get; } = CultureInfo.GetCultureInfo("en");

    public static CultureInfo Polish { get; } = CultureInfo.GetCultureInfo("pl");

    /// <summary>Polish when the operating system's UI is in Polish, English otherwise.</summary>
    public static CultureInfo FromOperatingSystem(CultureInfo osUiCulture) =>
        osUiCulture.TwoLetterISOLanguageName == Polish.TwoLetterISOLanguageName ? Polish : English;

    /// <summary>Makes <paramref name="language"/> the UI culture of every thread; call once at startup.</summary>
    public static void Apply(CultureInfo language)
    {
        CultureInfo.DefaultThreadCurrentUICulture = language;
        CultureInfo.CurrentUICulture = language;
    }
}
