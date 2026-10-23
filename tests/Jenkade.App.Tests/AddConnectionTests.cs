using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
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
            Find<MenuItem>(mainWindow, "AddConnectionMenuItem").RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            var form = Assert.IsType<AddConnectionWindow>(Assert.Single(mainWindow.OwnedWindows));

            Find<TextBox>(form, "DisplayName").Text = "Company Jenkins";
            Find<TextBox>(form, "Address").Text = "https://jenkins.example.com";
            Find<TextBox>(form, "UserName").Text = "anna";
            Find<TextBox>(form, "ApiToken").Text = "0000wrong0000";
            app.Jenkins.Serve("api/json", "unauthorized.http");
            Click(Find<Button>(form, "Save"));

            Assert.Equal(invalidCredentials, Find<TextBlock>(form, "Error").Text);
            Screenshots.Save(form, $"add-connection-invalid-credentials-{language}");

            Find<TextBox>(form, "ApiToken").Text = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";
            app.Jenkins.Serve("api/json", "root.http");
            Click(Find<Button>(form, "Save"));

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
        Find<ListBox>(window, "Connections").Items.Cast<Connection>().Select(connection => connection.DisplayName);

    // The automation peer clicks as the user does: the button's command runs, unlike raising ClickEvent.
    private static void Click(Button button)
    {
        ControlAutomationPeer.CreatePeerForElement(button).GetProvider<IInvokeProvider>()!.Invoke();
        Dispatcher.UIThread.RunJobs();
    }

    private static T Find<T>(ILogical root, string name) where T : Control =>
        root.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);
}
