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

    [Fact]
    public void Applying_a_language_keeps_the_regional_format()
    {
        var (culture, uiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        var (defaultCulture, defaultUiCulture) = (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            UiLanguage.Apply(UiLanguage.Polish);

            Assert.Equal("pl", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (culture, uiCulture);
            (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture) = (defaultCulture, defaultUiCulture);
        }
    }
}
