namespace Jenkade.Core;

/// <summary>What Jenkins answered when Jenkade checked a Connection's address, user name and API token.</summary>
public closed record ConnectionVerification
{
    public sealed record Verified : ConnectionVerification;

    /// <summary>Jenkins rejected the user name or API token (HTTP 401).</summary>
    public sealed record InvalidCredentials : ConnectionVerification;

    /// <summary>The user has no Overall/Read permission (HTTP 403).</summary>
    public sealed record NoPermission : ConnectionVerification;

    /// <summary>Nothing answered at the address: wrong host name or port, no network, or no response in time.</summary>
    public sealed record Unreachable : ConnectionVerification;

    /// <summary>The operating system's certificate store doesn't trust the address's TLS certificate.</summary>
    public sealed record UntrustedCertificate : ConnectionVerification;

    /// <summary>Something answered, but not Jenkins' API: another web server, or a path below Jenkins' root.</summary>
    public sealed record NotJenkins : ConnectionVerification;

    /// <summary>The address redirects elsewhere, e.g. from http to https; Jenkade doesn't follow it.</summary>
    public sealed record Redirected : ConnectionVerification;

    /// <summary>Jenkins answered with another error, e.g. HTTP 500 or a proxy's 502.</summary>
    public sealed record UnexpectedResponse : ConnectionVerification;
}
