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

        var saved = restarted.Load();
        Assert.Equal(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), saved);
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
    public void Keeps_the_saved_Connection_and_its_API_token_when_the_settings_file_cannot_be_written()
    {
        var connections = new ConnectionStore(settings.Folder, credentials);
        connections.Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), ApiToken);
        Directory.CreateDirectory(Path.Combine(settings.Folder.Path, "connection.json.tmp"));

        var saving = connections.Save(new Connection("", new Uri("https://ci.example.org/"), "anna.k"), "99f8e7d6c5b4a3928170f6e5d4c3b2a1f0");

        Assert.IsType<ConnectionSaving.SettingsFileNotWritten>(saving.Value);

        var restarted = new ConnectionStore(settings.Folder, credentials);
        Assert.Equal(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), restarted.Load());
        Assert.Equal(ApiToken, restarted.ApiToken());
    }

    [Fact]
    public void Reports_that_there_is_no_credential_store_instead_of_saving()
    {
        var connections = new ConnectionStore(settings.Folder, new UnavailableCredentialStore());

        var saving = connections.Save(new Connection("", new Uri("https://jenkins.example.com/"), "anna"), ApiToken);

        Assert.IsType<NoCredentialStore>(saving.Value);
        Assert.Equal(new SavedConnection.None(), connections.Load());
        Assert.IsType<NoCredentialStore>(connections.ApiToken().Value);
    }

    [Fact]
    public void Reports_the_credential_store_s_reason_when_it_fails()
    {
        var connections = new ConnectionStore(settings.Folder, credentials);
        credentials.Failing = true;

        var saving = connections.Save(new Connection("", new Uri("https://jenkins.example.com/"), "anna"), ApiToken);

        Assert.Equal(new CredentialStoreFailed("The parameter is incorrect."), saving.Value);
        Assert.Equal(new CredentialStoreFailed("The parameter is incorrect."), connections.ApiToken().Value);
        Assert.Equal(new SavedConnection.None(), connections.Load());
    }

    [Fact]
    public void Has_no_API_token_before_one_is_saved()
    {
        Assert.IsType<SavedApiToken.None>(new ConnectionStore(settings.Folder, credentials).ApiToken().Value);
    }

    [Fact]
    public void Has_no_Connection_on_the_first_run_before_the_settings_folder_exists()
    {
        var firstRun = new ConnectionStore(new SettingsFolder(Path.Combine(settings.Folder.Path, "Jenkade")), credentials);

        Assert.Equal(new SavedConnection.None(), firstRun.Load());
    }

    [Theory]
    [InlineData("")]
    [InlineData("{\"address\": \"https://jenkins.example.com/\", \"userName\":")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"address\": \"jenkins.example.com\", \"userName\": \"anna\"}")]
    [InlineData("{\"address\": \"ftp://jenkins.example.com/\", \"userName\": \"anna\"}")]
    [InlineData("{\"address\": \"https://jenkins.example.com/\", \"userName\": \" \"}")]
    [InlineData("{\"address\": \"https://jenkins.example.com/\"}")]
    public void A_damaged_settings_file_is_unreadable_instead_of_a_Connection(string settingsFile)
    {
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"), settingsFile);

        Assert.Equal(new SavedConnection.Unreadable(), new ConnectionStore(settings.Folder, credentials).Load());
    }

    [Fact]
    public void A_settings_file_another_program_holds_open_is_unreadable()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Only Windows locks files against reading.");
        using var locked = new FileStream(Path.Combine(settings.Folder.Path, "connection.json"), FileMode.Create, FileAccess.Write, FileShare.None);

        Assert.Equal(new SavedConnection.Unreadable(), new ConnectionStore(settings.Folder, credentials).Load());
    }

    [Fact]
    public void A_settings_file_without_a_display_name_has_a_Connection_named_by_its_address()
    {
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"),
            "{\"address\": \"https://jenkins.example.com/ci\", \"userName\": \"anna\"}");

        var saved = new ConnectionStore(settings.Folder, credentials).Load();

        Assert.Equal(new Connection("", new Uri("https://jenkins.example.com/ci/"), "anna"), saved);
    }

    public void Dispose() => settings.Dispose();
}
