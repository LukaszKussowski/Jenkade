using System.Diagnostics.CodeAnalysis;

namespace Jenkade.Core;

/// <summary>A user-configured access to one Jenkins server; its API token lives in the <see cref="ICredentialStore"/>.</summary>
public sealed record Connection(Guid Id, string DisplayName, Uri Address, string UserName)
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
