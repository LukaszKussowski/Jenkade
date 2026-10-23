using System.Net;
using System.Text;

namespace Jenkade.Testing;

/// <summary>
/// Reads a recording from <c>Recordings/</c>: a response as <c>curl -si</c> prints it, status line, headers,
/// blank line, body (docs/recording-jenkins.md).
/// </summary>
internal static class RecordedResponse
{
    private static readonly HashSet<string> TransferHeaders =
        new(["Content-Length", "Transfer-Encoding", "Content-Encoding"], StringComparer.OrdinalIgnoreCase);

    public static HttpResponseMessage Load(string name)
    {
        using var stream = typeof(RecordedResponse).Assembly.GetManifestResourceStream($"Recordings/{name}")
            ?? throw new ArgumentException($"No recording Recordings/{name}.", nameof(name));
        var text = new StreamReader(stream).ReadToEnd().ReplaceLineEndings("\n");
        var headerEnd = text.IndexOf("\n\n", StringComparison.Ordinal);
        var lines = text[..headerEnd].Split('\n');
        var body = text[(headerEnd + 2)..];

        // "HTTP/1.1 200 OK", or "HTTP/2 200" without a reason phrase.
        var status = lines[0].Split(' ', 3);
        var response = new HttpResponseMessage((HttpStatusCode)int.Parse(status[1]))
        {
            ReasonPhrase = status.ElementAtOrDefault(2),
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
        };
        foreach (var line in lines.Skip(1))
        {
            var separator = line.IndexOf(':');
            var (header, value) = (line[..separator], line[(separator + 1)..].Trim());
            // curl prints the body decoded, so these describe bytes the recording no longer holds.
            if (TransferHeaders.Contains(header))
                continue;
            if (!response.Headers.TryAddWithoutValidation(header, value))
                response.Content.Headers.TryAddWithoutValidation(header, value);
        }
        return response;
    }
}
