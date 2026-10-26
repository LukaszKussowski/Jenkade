using Jenkade.Testing;

namespace Jenkade.Core.Tests;

public sealed class ConnectionStoreTests : IDisposable
{
    private const string ApiToken = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";

    private readonly TemporarySettingsFolder settings = new();
    private readonly InMemoryCredentialStore credentials = new();

    [Fact]
    public void The_saved_Connection_survives_a_restart_with_its_API_token_outside_the_settings_folder()
    {
        new ConnectionStore(settings.Folder, credentials)
            .Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), ApiToken);

        var restarted = new ConnectionStore(settings.Folder, credentials);

        var connection = restarted.Load();
        Assert.Equal(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), connection);
        Assert.Equal(ApiToken, restarted.ApiToken());
        var settingsFile = Assert.Single(Directory.GetFiles(settings.Folder.Path));
        Assert.Equal("connection.json", Path.GetFileName(settingsFile));
        Assert.DoesNotContain(ApiToken, File.ReadAllText(settingsFile));
    }

    [Fact]
    public void Saving_a_Connection_replaces_the_one_saved_before_and_its_API_token()
    {
        var connections = new ConnectionStore(settings.Folder, credentials);
        connections.Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), ApiToken);

        connections.Save(new Connection("", new Uri("https://ci.example.org/"), "anna.k"), "99f8e7d6c5b4a3928170f6e5d4c3b2a1f0");

        var restarted = new ConnectionStore(settings.Folder, credentials);
        Assert.Equal(new Connection("", new Uri("https://ci.example.org/"), "anna.k"), restarted.Load());
        Assert.Equal("99f8e7d6c5b4a3928170f6e5d4c3b2a1f0", restarted.ApiToken());
    }

    [Fact]
    public void Has_no_Connection_on_the_first_run_before_the_settings_folder_exists()
    {
        var firstRun = new ConnectionStore(new SettingsFolder(Path.Combine(settings.Folder.Path, "Jenkade")), credentials);

        Assert.Null(firstRun.Load());
    }

    public void Dispose() => settings.Dispose();
}
