using System.Diagnostics.CodeAnalysis;

namespace Jenkade.Core;

/// <summary>
/// The user-configured access to the Jenkins server; Jenkade has at most one (ADR 0005). Its API token lives in the
/// <see cref="ICredentialStore"/>. <see cref="DisplayName"/> is an optional label and may be empty.
/// </summary>
public sealed record Connection(string DisplayName, Uri Address, string UserName)
{
    /// <summary>
    /// Accepts an absolute http or https address, e.g. <c>https://jenkins.example.com</c>, and ends its path with
    /// <c>/</c> so API paths resolve below it: <c>https://example.com/jenkins</c> becomes <c>https://example.com/jenkins/</c>.
    /// </summary>
    public static bool TryParseAddress(string text, [NotNullWhen(true)] out Uri? address)
    {
        address = null;
        if (!Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
            return false;
        var root = new UriBuilder(uri) { Query = "", Fragment = "" };
        if (!root.Path.EndsWith('/'))
            root.Path += "/";
        address = root.Uri;
        return true;
    }
}
