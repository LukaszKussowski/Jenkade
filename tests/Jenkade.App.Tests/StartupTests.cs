using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public class StartupTests
{
    [AvaloniaTheory]
    [InlineData("en", "_Help")]
    [InlineData("pl", "_Pomoc")]
    public void Main_window_opens_in_the_UI_language_without_binding_errors(string language, string helpMenu)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using var app = new TestSeams();
        using var host = app.CreateHost();

        var window = host.Services.GetRequiredService<MainWindow>();
        window.Show();
        Screenshots.Save(window, $"main-window-{language}");

        Assert.True(window.IsVisible);
        Assert.Equal("Jenkade", window.Title);
        Assert.Equal(helpMenu, Ui.Find<MenuItem>(window, "HelpMenuItem").Header);
        Assert.Empty(bindingErrors.Messages);
    }

    [AvaloniaTheory]
    [InlineData("en", "The saved Connection couldn't be read. Set it up again from the Connection menu.")]
    // Escaped: Polish text lives only in *.pl.resx (scripts/check-polish-text.py).
    [InlineData("pl", "Nie uda\u0142o si\u0119 odczyta\u0107 zapisanego po\u0142\u0105czenia. Skonfiguruj je ponownie w menu Po\u0142\u0105czenie.")]
    public void Main_window_opens_and_asks_to_set_up_the_Connection_again_when_its_settings_file_is_damaged(string language, string unreadable)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using var app = new TestSeams();
        File.WriteAllText(Path.Combine(app.Settings.Path, "connection.json"), "{\"address\":");
        using var host = app.CreateHost();

        var window = host.Services.GetRequiredService<MainWindow>();
        window.Show();
        Screenshots.Save(window, $"main-window-unreadable-connection-{language}");

        Assert.Equal(unreadable, Ui.Find<TextBlock>(window, "NoConnection").Text);
        Assert.Empty(bindingErrors.Messages);
    }

    [AvaloniaFact]
    public void Main_window_opens_with_the_platforms_own_credential_store()
    {
        using var app = new TestSeams();
        using var host = app.CreateHost(platformCredentialStore: true);

        var window = host.Services.GetRequiredService<MainWindow>();
        window.Show();

        Assert.True(window.IsVisible);
    }
}
