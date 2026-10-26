using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jenkade.Core;

/// <summary>
/// The user's one Connection (ADR 0005): settings without secrets in <c>connection.json</c> in the
/// <see cref="SettingsFolder"/>, the API token in the <see cref="ICredentialStore"/>.
/// </summary>
public sealed class ConnectionStore(SettingsFolder folder, ICredentialStore credentials)
{
    private const string CredentialKey = "Jenkade:Connection";

    private string SettingsFile => Path.Combine(folder.Path, "connection.json");

    /// <summary>The saved Connection, or null before the user has set one up.</summary>
    public Connection? Load()
    {
        if (!File.Exists(SettingsFile))
            return null;
        using var stream = File.OpenRead(SettingsFile);
        return JsonSerializer.Deserialize(stream, ConnectionJson.Default.Connection);
    }

    /// <summary>Saves the Connection and its API token in place of the ones saved before.</summary>
    public void Save(Connection connection, string apiToken)
    {
        credentials.Save(CredentialKey, apiToken);

        // Written next to the settings file and moved over it, so a crash never leaves it half-written.
        Directory.CreateDirectory(folder.Path);
        var temporaryFile = SettingsFile + ".tmp";
        using (var stream = File.Create(temporaryFile))
            JsonSerializer.Serialize(stream, connection, ConnectionJson.Default.Connection);
        File.Move(temporaryFile, SettingsFile, overwrite: true);
    }

    public string? ApiToken() => credentials.Read(CredentialKey);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(Connection))]
internal sealed partial class ConnectionJson : JsonSerializerContext;
