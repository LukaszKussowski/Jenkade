using Jenkade.Core;
using Jenkade.Testing;

namespace Jenkade.Presentation.Tests;

public sealed class MainWindowViewModelTests : IDisposable
{
    private readonly TemporarySettingsFolder settings = new();
    private readonly InMemoryCredentialStore credentials = new();
    private readonly Dialogs dialogs = new();

    [Fact]
    public void Shows_the_Connection_saved_in_an_earlier_run()
    {
        Connections().Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7");

        var main = new MainWindowViewModel(dialogs, Connections());

        Assert.Equal("Company Jenkins", main.ConnectionName);
    }

    [Fact]
    public void Names_a_Connection_without_a_display_name_by_its_address()
    {
        Connections().Save(new Connection("", new Uri("https://jenkins.example.com/ci/"), "anna"), "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7");

        var main = new MainWindowViewModel(dialogs, Connections());

        Assert.Equal("https://jenkins.example.com/ci/", main.ConnectionName);
    }

    [Fact]
    public void Has_no_Connection_before_the_user_sets_one_up()
    {
        var main = new MainWindowViewModel(dialogs, Connections());

        Assert.Null(main.Connection);
        Assert.False(main.SavedConnectionUnreadable);
    }

    [Fact]
    public async Task Shows_the_Connection_the_user_saved_in_the_Connection_settings()
    {
        var main = new MainWindowViewModel(dialogs, Connections());
        dialogs.SavedConnection = new Connection("Home Jenkins", new Uri("http://raspberrypi:8080/"), "anna");

        await main.ShowConnectionSettingsCommand.ExecuteAsync(null);

        Assert.Equal("Home Jenkins", main.ConnectionName);
    }

    [Fact]
    public async Task Keeps_the_Connection_when_the_user_closes_the_Connection_settings_without_saving()
    {
        Connections().Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7");
        var main = new MainWindowViewModel(dialogs, Connections());

        await main.ShowConnectionSettingsCommand.ExecuteAsync(null);

        Assert.Equal("Company Jenkins", main.ConnectionName);
    }

    [Fact]
    public void Tells_the_user_to_set_up_the_Connection_again_when_its_settings_file_is_damaged()
    {
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"), "{\"address\":");

        var main = new MainWindowViewModel(dialogs, Connections());

        Assert.Null(main.Connection);
        Assert.True(main.SavedConnectionUnreadable);
    }

    [Fact]
    public async Task Stops_telling_about_the_damaged_settings_file_once_the_user_saves_a_Connection()
    {
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"), "{\"address\":");
        var main = new MainWindowViewModel(dialogs, Connections());
        dialogs.SavedConnection = new Connection("Home Jenkins", new Uri("http://raspberrypi:8080/"), "anna");

        await main.ShowConnectionSettingsCommand.ExecuteAsync(null);

        Assert.False(main.SavedConnectionUnreadable);
        Assert.Equal("Home Jenkins", main.ConnectionName);
    }

    private ConnectionStore Connections() => new(settings.Folder, credentials);

    public void Dispose() => settings.Dispose();

    // Stands in for the windows (CODING_STANDARDS.md): returns what the user saved, or null when they closed the form.
    private sealed class Dialogs : IDialogService
    {
        public Connection? SavedConnection { get; set; }

        public Task ShowAboutAsync() => Task.CompletedTask;

        public Task<Connection?> ShowConnectionSettingsAsync() => Task.FromResult(SavedConnection);
    }
}
