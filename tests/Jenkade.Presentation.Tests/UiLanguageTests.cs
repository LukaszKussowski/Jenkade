using System.Globalization;

namespace Jenkade.Presentation.Tests;

public class UiLanguageTests
{
    [Theory]
    [InlineData("pl-PL", "pl")]
    [InlineData("pl", "pl")]
    [InlineData("en-US", "en")]
    [InlineData("en-GB", "en")]
    [InlineData("de-DE", "en")]
    [InlineData("", "en")]
    public void Follows_the_operating_system_UI_language_when_it_is_Polish_and_falls_back_to_English(string osUiLanguage, string expected)
    {
        var language = UiLanguage.FromOperatingSystem(CultureInfo.GetCultureInfo(osUiLanguage));

        Assert.Equal(expected, language.Name);
    }
}
