using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public sealed class ConnectionSettingsTests : IDisposable
{
    private readonly TestSeams app = new();

    [AvaloniaTheory]
    [InlineData("en", "Jenkins rejected the user name or API token.")]
    // Escaped: Polish text lives only in *.pl.resx (scripts/check-polish-text.py).
    [InlineData("pl", "Jenkins odrzuci\u0142 nazw\u0119 u\u017Cytkownika lub token API.")]
    public void Sets_up_the_Connection_once_Jenkins_verifies_it_and_replaces_it_after_a_restart(string language, string invalidCredentials)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using (var host = app.CreateHost())
        {
            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            var form = OpenConnectionSettings(mainWindow);

            Ui.Find<TextBox>(form, "DisplayName").Text = "Company Jenkins";
            Ui.Find<TextBox>(form, "Address").Text = "https://jenkins.example.com";
            Ui.Find<TextBox>(form, "UserName").Text = "anna";
            Ui.Find<TextBox>(form, "ApiToken").Text = "0000wrong0000";
            app.Jenkins.Serve("api/json", "unauthorized.http");
            Ui.Click(Ui.Find<Button>(form, "Save"));

            Assert.Equal(invalidCredentials, Ui.Find<TextBlock>(form, "Error").Text);
            Screenshots.Save(form, $"connection-settings-invalid-credentials-{language}");

            Ui.Find<TextBox>(form, "ApiToken").Text = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";
            app.Jenkins.Serve("api/json", "root.http");
            Ui.Click(Ui.Find<Button>(form, "Save"));

            Assert.False(form.IsVisible);
            Assert.Equal(("anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7"), app.Jenkins.Requests[^1].BasicCredentials);
            Assert.Equal("Company Jenkins", Ui.Find<TextBlock>(mainWindow, "ConnectionName").Text);
        }

        using var restarted = app.CreateHost();
        var restartedWindow = restarted.Services.GetRequiredService<MainWindow>();
        restartedWindow.Show();
        Screenshots.Save(restartedWindow, $"main-window-with-connection-{language}");
        Assert.Equal("Company Jenkins", Ui.Find<TextBlock>(restartedWindow, "ConnectionName").Text);

        var settings = OpenConnectionSettings(restartedWindow);
        Assert.Equal("https://jenkins.example.com/", Ui.Find<TextBox>(settings, "Address").Text);
        Screenshots.Save(settings, $"connection-settings-saved-{language}");
        Ui.Find<TextBox>(settings, "DisplayName").Text = "Company CI";
        Ui.Click(Ui.Find<Button>(settings, "Save"));

        Assert.Equal(("anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7"), app.Jenkins.Requests[^1].BasicCredentials);
        Assert.Equal("Company CI", Ui.Find<TextBlock>(restartedWindow, "ConnectionName").Text);
        Assert.Empty(bindingErrors.Messages);
    }

    public void Dispose() => app.Dispose();

    private static ConnectionSettingsWindow OpenConnectionSettings(MainWindow mainWindow)
    {
        Ui.Click(Ui.Find<MenuItem>(mainWindow, "ConnectionSettingsMenuItem"));
        return Assert.IsType<ConnectionSettingsWindow>(Assert.Single(mainWindow.OwnedWindows));
    }
}
