namespace Jenkade.Core;

/// <summary>What Jenkins answered when Jenkade checked a Connection's address, user name and API token.</summary>
public enum ConnectionVerification
{
    Verified,

    /// <summary>Jenkins rejected the user name or API token (HTTP 401).</summary>
    InvalidCredentials,

    /// <summary>The user has no Overall/Read permission (HTTP 403).</summary>
    NoPermission,

    /// <summary>Nothing answered at the address: wrong host name or port, no network, or no response in time.</summary>
    Unreachable,

    /// <summary>The operating system's certificate store doesn't trust the address's TLS certificate.</summary>
    UntrustedCertificate,

    /// <summary>Something answered, but not Jenkins' API: another web server, or a path below Jenkins' root.</summary>
    NotJenkins,

    /// <summary>The address redirects elsewhere, e.g. from http to https; Jenkade doesn't follow it.</summary>
    Redirected,

    /// <summary>Jenkins answered with another error, e.g. HTTP 500 or a proxy's 502.</summary>
    UnexpectedResponse,
}
