using System.Globalization;
using Jenkade.Core;
using Jenkade.Testing;

namespace Jenkade.Presentation.Tests;

public sealed class ConnectionSettingsViewModelTests : IDisposable
{
    private const string ApiToken = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";
    private const string EarlierApiToken = "99f8e7d6c5b4a3928170f6e5d4c3b2a1f0";

    private readonly FakeJenkins jenkins = new();
    private readonly TemporarySettingsFolder settings = new();
    private readonly InMemoryCredentialStore credentials = new();

    [Fact]
    public async Task Saves_a_Connection_that_Jenkins_verifies()
    {
        jenkins.Serve("api/json", "root.http");
        var form = FilledForm();
        Connection? saved = null;
        form.Saved += (_, connection) => saved = connection;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Null(form.Error);
        Assert.Equal(("anna", ApiToken), Assert.Single(jenkins.Requests).BasicCredentials);
        var expected = new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna");
        Assert.Equal(expected, saved);
        var restarted = new ConnectionStore(settings.Folder, credentials);
        Assert.Equal(expected, restarted.Load().Connection);
        Assert.Equal(ApiToken, restarted.ApiToken());
    }

    [Fact]
    public async Task Saves_a_Connection_without_a_display_name()
    {
        jenkins.Serve("api/json", "root.http");
        var form = FilledForm();
        form.DisplayName = " ";

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Null(form.Error);
        Assert.Equal("", Connections().Load().Connection?.DisplayName);
    }

    [Fact]
    public void Shows_the_saved_Connection_without_its_API_token()
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        SaveEarlierConnection();

        var form = EmptyForm();

        Assert.Equal(("Company Jenkins", "https://jenkins.example.com/", "anna", ""),
            (form.DisplayName, form.Address, form.UserName, form.ApiToken));
        Assert.Equal("Leave empty to keep the saved token", form.ApiTokenPlaceholder);
    }

    [Fact]
    public void Opens_empty_and_asks_for_an_API_token_when_the_settings_file_is_damaged()
    {
        SaveEarlierConnection();
        File.WriteAllText(Path.Combine(settings.Folder.Path, "connection.json"), "{}");

        var form = EmptyForm();

        Assert.Equal(("", "", "", ""), (form.DisplayName, form.Address, form.UserName, form.ApiToken));
        Assert.Equal("", form.ApiTokenPlaceholder);
        form.Address = "https://jenkins.example.com/";
        form.UserName = "anna";
        Assert.False(form.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task Replaces_the_saved_Connection_and_its_API_token()
    {
        SaveEarlierConnection();
        jenkins.Serve("ci/api/json", "root.http");
        var form = EmptyForm();
        form.Address = "https://jenkins.example.com/ci";
        form.ApiToken = ApiToken;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Null(form.Error);
        Assert.Equal(("anna", ApiToken), Assert.Single(jenkins.Requests).BasicCredentials);
        var restarted = Connections();
        Assert.Equal(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/ci/"), "anna"), restarted.Load().Connection);
        Assert.Equal(ApiToken, restarted.ApiToken());
    }

    [Fact]
    public async Task Keeps_the_saved_API_token_when_none_is_entered()
    {
        SaveEarlierConnection();
        jenkins.Serve("api/json", "root.http");
        var form = EmptyForm();
        form.DisplayName = "Company CI";

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Null(form.Error);
        Assert.Equal(("anna", EarlierApiToken), Assert.Single(jenkins.Requests).BasicCredentials);
        Assert.Equal("Company CI", Connections().Load().Connection?.DisplayName);
        Assert.Equal(EarlierApiToken, Connections().ApiToken());
    }

    [Fact]
    public async Task Saves_nothing_when_the_form_is_closed_while_Jenkins_has_not_answered_yet()
    {
        jenkins.NeverRespond();
        var form = FilledForm();
        var saved = false;
        form.Saved += (_, _) => saved = true;

        var saving = form.SaveCommand.ExecuteAsync(null);
        form.Close();
        await saving;

        Assert.False(saved);
        Assert.Null(form.Error);
        Assert.Null(Connections().Load().Connection);
    }

    [Theory]
    [InlineData(nameof(ConnectionSettingsViewModel.Address))]
    [InlineData(nameof(ConnectionSettingsViewModel.UserName))]
    [InlineData(nameof(ConnectionSettingsViewModel.ApiToken))]
    public void Can_be_saved_only_with_the_address_user_name_and_API_token_filled_in(string emptyField)
    {
        var form = FilledForm();
        Assert.True(form.SaveCommand.CanExecute(null));

        typeof(ConnectionSettingsViewModel).GetProperty(emptyField)!.SetValue(form, "  ");

        Assert.False(form.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void Can_be_saved_without_an_API_token_once_one_is_saved()
    {
        SaveEarlierConnection();

        Assert.True(EmptyForm().SaveCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("unauthorized", "Jenkins rejected the user name or API token.")]
    [InlineData("forbidden", "The user has no permission to read Jenkins (Overall/Read).")]
    [InlineData("refused", "Nothing answers at this address. Check the address and your network access.")]
    [InlineData("untrusted-certificate",
        "The operating system doesn't trust this address's certificate. Add its certificate authority to the system certificate store.")]
    [InlineData("not-jenkins", "There's no Jenkins at this address.")]
    [InlineData("redirect-to-https", "This address redirects elsewhere. Enter the address Jenkins is served at, e.g. with https://.")]
    [InlineData("internal-server-error", "Jenkins answered with an error. Try again later.")]
    public async Task Explains_why_Jenkins_did_not_verify_the_Connection_and_keeps_the_saved_one(string jenkinsBehaviour, string expectedError)
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        SaveEarlierConnection();
        _ = jenkinsBehaviour switch
        {
            "refused" => jenkins.RefuseConnections(),
            "untrusted-certificate" => jenkins.PresentUntrustedCertificate(),
            _ => jenkins.Serve("api/json", $"{jenkinsBehaviour}.http"),
        };
        var form = EmptyForm();
        form.UserName = "anna.k";
        form.ApiToken = ApiToken;
        var saved = false;
        form.Saved += (_, _) => saved = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expectedError, form.Error);
        Assert.False(saved);
        Assert.Equal("anna", Connections().Load().Connection?.UserName);
        Assert.Equal(EarlierApiToken, Connections().ApiToken());
    }

    [Fact]
    public async Task Explains_that_the_Connection_cannot_be_saved_without_a_credential_store()
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        jenkins.Serve("api/json", "root.http");
        var connections = new ConnectionStore(settings.Folder, new UnavailableCredentialStore());
        var form = FilledForm(connections);
        var saved = false;
        form.Saved += (_, _) => saved = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal("This system has no credential store to keep the API token in, so the Connection can't be saved.", form.Error);
        Assert.False(saved);
        Assert.Null(connections.Load().Connection);
    }

    [Fact]
    public async Task Explains_that_the_credential_store_failed_to_keep_the_API_token()
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        jenkins.Serve("api/json", "root.http");
        var form = FilledForm();
        var saved = false;
        form.Saved += (_, _) => saved = true;
        credentials.Failing = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal("The Connection couldn't be saved: The parameter is incorrect.", form.Error);
        Assert.False(saved);
    }

    [Fact]
    public async Task Explains_that_the_credential_store_failed_to_read_the_saved_API_token()
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        SaveEarlierConnection();
        var form = EmptyForm();
        credentials.Failing = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal("The Connection couldn't be saved: The parameter is incorrect.", form.Error);
        Assert.Empty(jenkins.Requests);
    }

    [Fact]
    public async Task Explains_that_the_settings_file_could_not_be_written_and_keeps_the_saved_Connection()
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        SaveEarlierConnection();
        Directory.CreateDirectory(Path.Combine(settings.Folder.Path, "connection.json.tmp"));
        jenkins.Serve("api/json", "root.http");
        var form = FilledForm();
        form.DisplayName = "Company CI";
        var saved = false;
        form.Saved += (_, _) => saved = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.StartsWith("The Connection couldn't be saved: ", form.Error);
        Assert.False(saved);
        Assert.Equal("Company Jenkins", Connections().Load().Connection?.DisplayName);
        Assert.Equal(EarlierApiToken, Connections().ApiToken());
    }

    [Theory]
    [InlineData("jenkins.example.com")]
    [InlineData("ftp://jenkins.example.com/")]
    [InlineData("https://")]
    public async Task Asks_for_a_full_http_address_without_calling_Jenkins(string address)
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        var form = FilledForm();
        form.Address = address;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Enter the full address, starting with https:// or http://.", form.Error);
        Assert.Empty(jenkins.Requests);
        Assert.Null(Connections().Load().Connection);
    }

    private ConnectionStore Connections() => new(settings.Folder, credentials);

    private void SaveEarlierConnection() =>
        Connections().Save(new Connection("Company Jenkins", new Uri("https://jenkins.example.com/"), "anna"), EarlierApiToken);

    private ConnectionSettingsViewModel EmptyForm() => new(new JenkinsClient(jenkins, TimeProvider.System), Connections());

    private ConnectionSettingsViewModel FilledForm() => FilledForm(Connections());

    private ConnectionSettingsViewModel FilledForm(ConnectionStore connections) =>
        new(new JenkinsClient(jenkins, TimeProvider.System), connections)
        {
            DisplayName = "Company Jenkins",
            Address = "https://jenkins.example.com",
            UserName = "anna",
            ApiToken = ApiToken,
        };

    public void Dispose() => settings.Dispose();
}
