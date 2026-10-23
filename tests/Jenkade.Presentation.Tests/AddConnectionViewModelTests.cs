using System.Globalization;
using Jenkade.Core;
using Jenkade.Testing;

namespace Jenkade.Presentation.Tests;

public sealed class AddConnectionViewModelTests : IDisposable
{
    private const string ApiToken = "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7";

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
        Assert.NotNull(saved);
        Assert.Equal(("anna", ApiToken), Assert.Single(jenkins.Requests).BasicCredentials);
        var restarted = new ConnectionStore(settings.Folder, credentials);
        var connection = Assert.Single(restarted.All());
        Assert.Equal(saved, connection);
        Assert.Equal(("Company Jenkins", "https://jenkins.example.com/", "anna"),
            (connection.DisplayName, connection.Address.ToString(), connection.UserName));
        Assert.Equal(ApiToken, restarted.ApiToken(connection));
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
        Assert.Empty(new ConnectionStore(settings.Folder, credentials).All());
    }

    [Theory]
    [InlineData(nameof(AddConnectionViewModel.DisplayName))]
    [InlineData(nameof(AddConnectionViewModel.Address))]
    [InlineData(nameof(AddConnectionViewModel.UserName))]
    [InlineData(nameof(AddConnectionViewModel.ApiToken))]
    public void Can_be_saved_only_with_every_field_filled_in(string emptyField)
    {
        var form = FilledForm();
        Assert.True(form.SaveCommand.CanExecute(null));

        typeof(AddConnectionViewModel).GetProperty(emptyField)!.SetValue(form, "  ");

        Assert.False(form.SaveCommand.CanExecute(null));
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
    public async Task Explains_why_Jenkins_did_not_verify_the_Connection_and_saves_nothing(string jenkinsBehaviour, string expectedError)
    {
        CultureInfo.CurrentUICulture = UiLanguage.English;
        _ = jenkinsBehaviour switch
        {
            "refused" => jenkins.RefuseConnections(),
            "untrusted-certificate" => jenkins.PresentUntrustedCertificate(),
            _ => jenkins.Serve("api/json", $"{jenkinsBehaviour}.http"),
        };
        var form = FilledForm();
        var saved = false;
        form.Saved += (_, _) => saved = true;

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expectedError, form.Error);
        Assert.False(saved);
        Assert.Empty(new ConnectionStore(settings.Folder, credentials).All());
    }

    [Fact]
    public async Task Verifies_and_saves_a_Jenkins_that_lives_under_a_path()
    {
        jenkins.Serve("ci/api/json", "root.http");
        var form = FilledForm();
        form.Address = "https://jenkins.example.com/ci";

        await form.SaveCommand.ExecuteAsync(null);

        Assert.Null(form.Error);
        Assert.Equal("/ci/api/json", Assert.Single(jenkins.Requests).Uri.AbsolutePath);
        Assert.Equal("https://jenkins.example.com/ci/", Assert.Single(new ConnectionStore(settings.Folder, credentials).All()).Address.ToString());
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
        Assert.Empty(new ConnectionStore(settings.Folder, credentials).All());
    }

    private AddConnectionViewModel FilledForm() =>
        new(new JenkinsClient(jenkins, TimeProvider.System), new ConnectionStore(settings.Folder, credentials))
        {
            DisplayName = "Company Jenkins",
            Address = "https://jenkins.example.com",
            UserName = "anna",
            ApiToken = ApiToken,
        };

    public void Dispose() => settings.Dispose();
}
