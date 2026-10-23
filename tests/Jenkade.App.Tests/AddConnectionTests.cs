using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Jenkade.App.Views;
using Jenkade.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public sealed class AddConnectionTests : IDisposable
{
    private readonly TestSeams app = new();

    [AvaloniaTheory]
    [InlineData("en", "Jenkins rejected the user name or API token.")]
    // Escaped: Polish text lives only in *.pl.resx (scripts/check-polish-text.py).
    [InlineData("pl", "Jenkins odrzuci\u0142 nazw\u0119 u\u017Cytkownika lub token API.")]
    public void Adds_a_Connection_once_Jenkins_verifies_it_and_keeps_it_after_a_restart(string language, string invalidCredentials)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using (var host = app.CreateHost())
        {
            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Ui.Click(Ui.Find<MenuItem>(mainWindow, "AddConnectionMenuItem"));
            var form = Assert.IsType<AddConnectionWindow>(Assert.Single(mainWindow.OwnedWindows));

            Ui.Find<TextBox>(form, "DisplayName").Text = "Company Jenkins";
            Ui.Find<TextBox>(form, "Address").Text = "https://jenkins.example.com";
            Ui.Find<TextBox>(form, "UserName").Text = "anna";
            Ui.Find<TextBox>(form, "ApiToken").Text = "0000wrong0000";
            app.Jenkins.Serve("api/json", "unauthorized.http");
            Ui.Click(Ui.Find<Button>(form, "Save"));

            Assert.Equal(invalidCredentials, Ui.Find<TextBlock>(form, "Error").Text);
            Screenshots.Save(form, $"add-connection-invalid-credentials-{language}");

            Ui.Find<TextBox>(form, "ApiToken").Text = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";
            app.Jenkins.Serve("api/json", "root.http");
            Ui.Click(Ui.Find<Button>(form, "Save"));

            Assert.False(form.IsVisible);
            Assert.Equal(("anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7"), app.Jenkins.Requests[^1].BasicCredentials);
            Assert.Equal(["Company Jenkins"], ConnectionNames(mainWindow));
        }

        using var restarted = app.CreateHost();
        var restartedWindow = restarted.Services.GetRequiredService<MainWindow>();
        restartedWindow.Show();
        Screenshots.Save(restartedWindow, $"main-window-with-connection-{language}");
        Assert.Equal(["Company Jenkins"], ConnectionNames(restartedWindow));
        Assert.Empty(bindingErrors.Messages);
    }

    public void Dispose() => app.Dispose();

    private static IEnumerable<string> ConnectionNames(MainWindow window) =>
        Ui.Find<ListBox>(window, "Connections").Items.Cast<Connection>().Select(connection => connection.DisplayName);
}
