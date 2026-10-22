using System.Globalization;

namespace Jenkade.Presentation.Tests;

public class DisplayFormatTests
{
    private static readonly DateTimeOffset BuildStart = new(2026, 10, 22, 14, 5, 0, TimeSpan.FromHours(2));

    [Fact]
    public void Shows_a_timestamp_in_Polish()
    {
        using var _ = new LanguageScope("pl");

        Assert.Equal("22.10.2026 14:05", DisplayFormat.Timestamp(BuildStart));
    }

    [Fact]
    public void Shows_a_timestamp_in_English()
    {
        using var _ = new LanguageScope("en");

        // ICU separates "PM" with a narrow no-break space or a plain one, depending on its version.
        Assert.Matches(@"^10/22/2026 2:05\sPM$", DisplayFormat.Timestamp(BuildStart));
    }

    [Theory]
    [InlineData("en", 0, 0, 0.4, "0 s")]
    [InlineData("en", 0, 0, 42, "42 s")]
    [InlineData("en", 0, 3, 7, "3 min 7 s")]
    [InlineData("en", 2, 15, 59, "2 h 15 min")]
    [InlineData("en", 26, 0, 0, "26 h 0 min")]
    [InlineData("pl", 0, 0, 42, "42 s")]
    [InlineData("pl", 0, 3, 7, "3 min 7 s")]
    [InlineData("pl", 2, 15, 59, "2 godz. 15 min")]
    public void Shows_a_duration_in_its_two_largest_units(string language, int hours, int minutes, double seconds, string expected)
    {
        using var _ = new LanguageScope(language);

        var duration = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

        Assert.Equal(expected, DisplayFormat.Duration(duration));
    }

    private sealed class LanguageScope : IDisposable
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
}
