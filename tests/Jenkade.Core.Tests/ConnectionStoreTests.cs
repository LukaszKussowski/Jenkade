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

    public static TheoryData<string, SettingsFileFault> DamagedSettingsFiles => new()
    {
        { "", new SettingsFileFault.InvalidJson(Line: 1) },
        { "{\"address\": \"https://jenkins.example.com/\", \"userName\":", new SettingsFileFault.InvalidJson(Line: 1) },
        { "{\n  \"address\": \"https://jenkins.example.com/\",\n  \"userName\" \"anna\"\n}", new SettingsFileFault.InvalidJson(Line: 3) },
        { "null", new SettingsFileFault.MissingAddress() },
        { "{}", new SettingsFileFault.MissingAddress() },
        { "{\"address\": \" \", \"userName\": \"anna\"}", new SettingsFileFault.MissingAddress() },
        { "{\"address\": \"jenkins.example.com\", \"userName\": \"anna\"}", new SettingsFileFault.InvalidAddress("jenkins.example.com") },
        { "{\"address\": \"ftp://jenkins.example.com/\", \"userName\": \"anna\"}", new SettingsFileFault.InvalidAddress("ftp://jenkins.example.com/") },
        { "{\"address\": \"https://jenkins.example.com/\", \"userName\": \" \"}", new SettingsFileFault.MissingUserName() },
        { "{\"address\": \"https://jenkins.example.com/\"}", new SettingsFileFault.MissingUserName() },
    };

    [Theory]
    [MemberData(nameof(DamagedSettingsFiles))]
    public void A_damaged_settings_file_is_unreadable_and_says_why(string settingsFile, SettingsFileFault fault)
    {
        File.WriteAllText(SettingsFile, settingsFile);

        Assert.Equal(new SavedConnection.Unreadable(SettingsFile, fault), new ConnectionStore(settings.Folder, credentials).Load());
    }

    [Fact]
    public void A_settings_file_that_cannot_be_opened_is_unreadable_with_the_system_s_reason()
    {
        File.WriteAllText(SettingsFile, "{}");
        using var locked = OperatingSystem.IsWindows()
            ? new FileStream(SettingsFile, FileMode.Open, FileAccess.Write, FileShare.None)
            : null;
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(SettingsFile, UnixFileMode.None);

        var unreadable = Assert.IsType<SavedConnection.Unreadable>(new ConnectionStore(settings.Folder, credentials).Load().Value);

        Assert.Equal(SettingsFile, unreadable.File);
        var cannotOpen = Assert.IsType<SettingsFileFault.CannotOpen>(unreadable.Fault);
        Assert.Contains("connection.json", cannotOpen.SystemReason);
    }

    [Fact]
    public void A_settings_file_without_a_display_name_has_a_Connection_named_by_its_address()
    {
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"),
            "{\"address\": \"https://jenkins.example.com/ci\", \"userName\": \"anna\"}");

        var saved = new ConnectionStore(settings.Folder, credentials).Load();

        Assert.Equal(new Connection("", new Uri("https://jenkins.example.com/ci/"), "anna"), saved);
    }

    private string SettingsFile => Path.Combine(settings.Folder.Path, "connection.json");

    public void Dispose() => settings.Dispose();
}
