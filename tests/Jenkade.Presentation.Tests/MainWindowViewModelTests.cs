using Jenkade.Core;
using Jenkade.Testing;

namespace Jenkade.Presentation.Tests;

public sealed class MainWindowViewModelTests : IDisposable
{
    private readonly TemporarySettingsFolder settings = new();
    private readonly InMemoryCredentialStore credentials = new();

    [Fact]
    public void Lists_the_Connections_saved_in_an_earlier_run()
    {
        var earlierRun = new ConnectionStore(settings.Folder, credentials);
        earlierRun.Add("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7");
        earlierRun.Add("Home Jenkins", new Uri("http://raspberrypi:8080/"), "anna", "99f8e7d6c5b4a3928170f6e5d4c3b2a1f0");

        var main = new MainWindowViewModel(new NoDialogs(), new ConnectionStore(settings.Folder, credentials));

        Assert.Equal(["Company Jenkins", "Home Jenkins"], main.Connections.Select(connection => connection.DisplayName));
    }

    public void Dispose() => settings.Dispose();

    private sealed class NoDialogs : IDialogService
    {
        public Task ShowAboutAsync() => throw new InvalidOperationException("No dialog expected.");

        public Task<Connection?> ShowAddConnectionAsync() => throw new InvalidOperationException("No dialog expected.");
    }
}
