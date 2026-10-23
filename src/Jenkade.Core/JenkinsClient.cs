using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;

namespace Jenkade.Core;

/// <summary>Talks to Jenkins servers over their REST API, authenticated with a user name and API token.</summary>
public sealed class JenkinsClient(HttpMessageHandler handler, TimeProvider time)
{
    private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);

    // HttpClient's own timeout runs on real time; requests time out on the injected TimeProvider instead.
    private readonly HttpClient http = new(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };

    public async Task<ConnectionVerification> VerifyAsync(Uri address, string userName, string apiToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(address, "api/json?tree=mode"));
        request.Headers.Authorization = BasicAuth(userName, apiToken);
        using var timeout = new CancellationTokenSource(ResponseTimeout, time);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellation.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return ConnectionVerification.Unreachable;
        }
        catch (HttpRequestException exception) when (exception.InnerException is AuthenticationException)
        {
            // Trust is the operating system's decision (no custom validation callback): there's no way to ignore it.
            return ConnectionVerification.UntrustedCertificate;
        }
        catch (HttpRequestException)
        {
            return ConnectionVerification.Unreachable;
        }

        using var _ = response;
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => ConnectionVerification.InvalidCredentials,
            HttpStatusCode.Forbidden => ConnectionVerification.NoPermission,
            HttpStatusCode.NotFound => ConnectionVerification.NotJenkins,
            >= HttpStatusCode.MultipleChoices and < HttpStatusCode.BadRequest => ConnectionVerification.Redirected,
            _ when !response.IsSuccessStatusCode => ConnectionVerification.UnexpectedResponse,
            // Every Jenkins response carries its version in X-Jenkins.
            _ when !response.Headers.Contains("X-Jenkins") => ConnectionVerification.NotJenkins,
            _ => ConnectionVerification.Verified,
        };
    }

    // Sent with the first request: Jenkins that allows anonymous reading never asks for credentials.
    private static AuthenticationHeaderValue BasicAuth(string userName, string apiToken) =>
        new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{apiToken}")));
}
