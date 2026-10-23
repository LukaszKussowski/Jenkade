using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;

namespace Jenkade.Testing;

/// <summary>
/// Stands in for a Jenkins server at <see cref="Address"/>: serves recorded responses by path and records every request.
/// Paths without a recording get Jenkins' 404; other hosts and ports refuse the connection.
/// </summary>
public sealed class FakeJenkins : HttpMessageHandler
{
    public static readonly Uri Address = new("https://jenkins.example.com/");

    private readonly Dictionary<string, string> recordings = [];
    private readonly List<RecordedRequest> requests = [];
    private Func<CancellationToken, Task<HttpResponseMessage>>? failure;

    public IReadOnlyList<RecordedRequest> Requests => requests;

    /// <summary>Answers requests for <paramref name="path"/> (relative to <see cref="Address"/>, query ignored) with a recording.</summary>
    public FakeJenkins Serve(string path, string recording)
    {
        recordings[new Uri(Address, path).AbsolutePath] = recording;
        return this;
    }

    /// <summary>Nothing listens at the address: every request fails as a refused connection does.</summary>
    public FakeJenkins RefuseConnections() => FailWith(ConnectionRefused(Address));

    /// <summary>The TLS handshake fails as it does for a certificate the operating system doesn't trust.</summary>
    public FakeJenkins PresentUntrustedCertificate() => FailWith(new HttpRequestException(
        HttpRequestError.SecureConnectionError, "The SSL connection could not be established, see inner exception.",
        new AuthenticationException("The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot")));

    /// <summary>Accepts the connection but never answers, until the request is cancelled.</summary>
    public FakeJenkins NeverRespond()
    {
        failure = async cancellationToken =>
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new UnreachableException();
        };
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        requests.Add(new RecordedRequest(request.Method, request.RequestUri!, request.Headers.Authorization));
        if (request.RequestUri!.Authority != Address.Authority)
            throw ConnectionRefused(request.RequestUri);
        if (failure is not null)
            return await failure(cancellationToken);

        var recording = recordings.GetValueOrDefault(request.RequestUri!.AbsolutePath, "not-found.http");
        var response = RecordedResponse.Load(recording);
        response.RequestMessage = request;
        return response;
    }

    private static HttpRequestException ConnectionRefused(Uri uri) => new(
        HttpRequestError.ConnectionError, $"Connection refused ({uri.Host}:{uri.Port})", new IOException("Connection refused"));

    private FakeJenkins FailWith(HttpRequestException exception)
    {
        failure = _ => Task.FromException<HttpResponseMessage>(exception);
        return this;
    }
}

public sealed record RecordedRequest(HttpMethod Method, Uri Uri, AuthenticationHeaderValue? Authorization)
{
    /// <summary>The user name and API token of a Basic <c>Authorization</c> header, or null without one.</summary>
    public (string UserName, string ApiToken)? BasicCredentials
    {
        get
        {
            if (Authorization is not { Scheme: "Basic", Parameter: { } parameter })
                return null;
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parameter));
            var separator = decoded.IndexOf(':');
            return (decoded[..separator], decoded[(separator + 1)..]);
        }
    }
}
