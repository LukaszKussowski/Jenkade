using System.Net;
using System.Text;

namespace Jenkade.Testing;

/// <summary>
/// Reads a recording from <c>Recordings/</c>: an HTTP/1.1 response as it came over the wire,
/// status line, headers, blank line, body.
/// </summary>
internal static class RecordedResponse
{
    public static HttpResponseMessage Load(string name)
    {
        using var stream = typeof(RecordedResponse).Assembly.GetManifestResourceStream($"Recordings/{name}")
            ?? throw new ArgumentException($"No recording Recordings/{name}.", nameof(name));
        var text = new StreamReader(stream).ReadToEnd().ReplaceLineEndings("\n");
        var headerEnd = text.IndexOf("\n\n", StringComparison.Ordinal);
        var lines = text[..headerEnd].Split('\n');
        var body = text[(headerEnd + 2)..];

        var status = lines[0].Split(' ', 3);
        var response = new HttpResponseMessage((HttpStatusCode)int.Parse(status[1]))
        {
            ReasonPhrase = status[2],
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
        };
        foreach (var line in lines.Skip(1))
        {
            var separator = line.IndexOf(':');
            var (header, value) = (line[..separator], line[(separator + 1)..].Trim());
            if (!response.Headers.TryAddWithoutValidation(header, value))
                response.Content.Headers.TryAddWithoutValidation(header, value);
        }
        return response;
    }
}
