using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jenkade.Core;

/// <summary>
/// The user's Connections: settings without secrets in <c>connections.json</c> in the <see cref="SettingsFolder"/>,
/// API tokens in the <see cref="ICredentialStore"/>.
/// </summary>
public sealed class ConnectionStore(SettingsFolder folder, ICredentialStore credentials)
{
    private string SettingsFile => Path.Combine(folder.Path, "connections.json");

    public IReadOnlyList<Connection> All()
    {
        if (!File.Exists(SettingsFile))
            return [];
        using var stream = File.OpenRead(SettingsFile);
        return JsonSerializer.Deserialize(stream, ConnectionSettingsJson.Default.ConnectionSettings)?.Connections ?? [];
    }

    public Connection Add(string displayName, Uri address, string userName, string apiToken)
    {
        var connection = new Connection(Guid.NewGuid(), displayName, address, userName);
        credentials.Save(CredentialKey(connection), apiToken);
        Save(new ConnectionSettings([.. All(), connection]));
        return connection;
    }

    public string? ApiToken(Connection connection) => credentials.Read(CredentialKey(connection));

    private static string CredentialKey(Connection connection) => $"Jenkade:Connection:{connection.Id}";

    // Written next to the settings file and moved over it, so a crash never leaves it half-written.
    private void Save(ConnectionSettings settings)
    {
        Directory.CreateDirectory(folder.Path);
        var temporaryFile = SettingsFile + ".tmp";
        using (var stream = File.Create(temporaryFile))
            JsonSerializer.Serialize(stream, settings, ConnectionSettingsJson.Default.ConnectionSettings);
        File.Move(temporaryFile, SettingsFile, overwrite: true);
    }
}

internal sealed record ConnectionSettings(IReadOnlyList<Connection> Connections);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(ConnectionSettings))]
internal sealed partial class ConnectionSettingsJson : JsonSerializerContext;
