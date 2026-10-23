using System.Globalization;

namespace Jenkade.Presentation.Tests;

// UiLanguage.Apply changes the default culture of every thread; other tests running in parallel would see it.
[CollectionDefinition(nameof(ProcessWideCulture), DisableParallelization = true)]
public sealed class ProcessWideCulture;

[Collection(nameof(ProcessWideCulture))]
public class UiLanguageApplyTests
{
    [Fact]
    public void Applying_a_language_changes_the_UI_text_of_every_thread_and_keeps_the_regional_format()
    {
        var (culture, uiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        var (defaultCulture, defaultUiCulture) = (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            UiLanguage.Apply(UiLanguage.Polish);

            Assert.Equal("pl", CultureInfo.CurrentUICulture.Name);
            Assert.Equal("pl", CultureInfo.DefaultThreadCurrentUICulture?.Name);
            Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
            Assert.Same(defaultCulture, CultureInfo.DefaultThreadCurrentCulture);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (culture, uiCulture);
            (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture) = (defaultCulture, defaultUiCulture);
        }
    }
}
