using Jenkade.Testing;

namespace Jenkade.Core.Tests;

public sealed class ConnectionStoreTests : IDisposable
{
    private const string ApiToken = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";

    private readonly TemporarySettingsFolder settings = new();
    private readonly InMemoryCredentialStore credentials = new();

    [Fact]
    public void An_added_Connection_survives_a_restart_with_its_API_token_outside_the_settings_folder()
    {
        new ConnectionStore(settings.Folder, credentials)
            .Add("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna", ApiToken);

        var restarted = new ConnectionStore(settings.Folder, credentials);

        var connection = Assert.Single(restarted.All());
        Assert.Equal("Company Jenkins", connection.DisplayName);
        Assert.Equal(new Uri("https://jenkins.example.com/"), connection.Address);
        Assert.Equal("anna", connection.UserName);
        Assert.Equal(ApiToken, restarted.ApiToken(connection));
        var settingsFile = Assert.Single(Directory.GetFiles(settings.Folder.Path));
        Assert.Equal("connections.json", Path.GetFileName(settingsFile));
        Assert.DoesNotContain(ApiToken, File.ReadAllText(settingsFile));
    }

    [Fact]
    public void Has_no_Connections_on_the_first_run_before_the_settings_folder_exists()
    {
        var firstRun = new ConnectionStore(new SettingsFolder(Path.Combine(settings.Folder.Path, "Jenkade")), credentials);

        Assert.Empty(firstRun.All());
    }

    public void Dispose() => settings.Dispose();
}
