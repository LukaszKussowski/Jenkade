using Jenkade.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Jenkade.Core.Tests;

public class ConnectionVerificationTests
{
    private readonly FakeJenkins jenkins = new();
    private readonly FakeTimeProvider time = new();

    [Fact]
    public async Task Verifies_a_Connection_with_Basic_auth_of_the_user_name_and_API_token()
    {
        jenkins.Serve("api/json", "root.http");

        var verification = await Verify();

        Assert.Equal(ConnectionVerification.Verified, verification);
        var request = Assert.Single(jenkins.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://jenkins.example.com/api/json", request.Uri.GetLeftPart(UriPartial.Path));
        Assert.Equal(("anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7"), request.BasicCredentials);
    }

    [Fact]
    public async Task Reports_invalid_credentials_when_Jenkins_rejects_the_API_token()
    {
        jenkins.Serve("api/json", "unauthorized.http");

        Assert.Equal(ConnectionVerification.InvalidCredentials, await Verify());
    }

    [Fact]
    public async Task Reports_no_permission_when_the_user_may_not_read_Jenkins()
    {
        jenkins.Serve("api/json", "forbidden.http");

        Assert.Equal(ConnectionVerification.NoPermission, await Verify());
    }

    [Fact]
    public async Task Reports_an_unreachable_address_when_nothing_answers_there()
    {
        jenkins.RefuseConnections();

        Assert.Equal(ConnectionVerification.Unreachable, await Verify());
    }

    [Fact]
    public async Task Reports_an_untrusted_certificate_when_the_operating_system_does_not_trust_it()
    {
        jenkins.PresentUntrustedCertificate();

        Assert.Equal(ConnectionVerification.UntrustedCertificate, await Verify());
    }

    [Theory]
    [InlineData("not-jenkins.http")]
    [InlineData("not-found.http")]
    public async Task Reports_no_Jenkins_when_the_address_serves_something_else(string recording)
    {
        jenkins.Serve("api/json", recording);

        Assert.Equal(ConnectionVerification.NotJenkins, await Verify());
    }

    [Fact]
    public async Task Reports_an_unexpected_response_when_Jenkins_fails_otherwise()
    {
        jenkins.Serve("api/json", "internal-server-error.http");

        Assert.Equal(ConnectionVerification.UnexpectedResponse, await Verify());
    }

    [Fact]
    public async Task Reports_an_unreachable_address_when_nothing_answers_within_30_seconds()
    {
        jenkins.NeverRespond();

        var verification = Verify();
        time.Advance(TimeSpan.FromSeconds(29));
        Assert.False(verification.IsCompleted);
        time.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(ConnectionVerification.Unreachable, await verification);
    }

    private Task<ConnectionVerification> Verify() =>
        new JenkinsClient(jenkins, time)
            .VerifyAsync(FakeJenkins.Address, "anna", "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7", TestContext.Current.CancellationToken);
}
