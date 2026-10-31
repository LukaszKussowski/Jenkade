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

    /// <summary>
    /// The saved Connection; none before the user has set one up, and none but <see cref="SavedConnection.Unreadable"/>
    /// when <c>connection.json</c> can't be read or doesn't describe a Connection. Saving one replaces the file.
    /// </summary>
    public SavedConnection Load()
    {
        if (!File.Exists(SettingsFile))
            return new SavedConnection(null, Unreadable: false);
        try
        {
            using var stream = File.OpenRead(SettingsFile);
            // Read field by field: the deserializer leaves missing fields null instead of failing.
            if (JsonSerializer.Deserialize(stream, ConnectionJson.Default.ConnectionFile) is { Address: { } address, UserName: { } userName } file
                && Connection.TryParseAddress(address, out var parsedAddress) && !string.IsNullOrWhiteSpace(userName))
                return new SavedConnection(new Connection(file.DisplayName ?? "", parsedAddress, userName), Unreadable: false);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
        }
        return new SavedConnection(null, Unreadable: true);
    }

    /// <summary>
    /// Saves the Connection and its API token in place of the ones saved before, or keeps those and throws a
    /// <see cref="CredentialStoreException"/> or a <see cref="ConnectionNotSavedException"/>.
    /// </summary>
    public void Save(Connection connection, string apiToken)
    {
        var previousApiToken = credentials.Read(CredentialKey);
        credentials.Save(CredentialKey, apiToken);
        try
        {
            // Written next to the settings file and moved over it, so a crash never leaves it half-written.
            Directory.CreateDirectory(folder.Path);
            var temporaryFile = SettingsFile + ".tmp";
            using (var stream = File.Create(temporaryFile))
                JsonSerializer.Serialize(stream, new ConnectionFile(connection.DisplayName, connection.Address.AbsoluteUri, connection.UserName),
                    ConnectionJson.Default.ConnectionFile);
            File.Move(temporaryFile, SettingsFile, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // The saved Connection stays, so it keeps its own API token.
            if (previousApiToken is not null)
                credentials.Save(CredentialKey, previousApiToken);
            throw new ConnectionNotSavedException(exception);
        }
    }

    public string? ApiToken() => credentials.Read(CredentialKey);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(ConnectionFile))]
internal sealed partial class ConnectionJson : JsonSerializerContext;

/// <summary>The contents of <c>connection.json</c>, each field possibly missing.</summary>
internal sealed record ConnectionFile(string? DisplayName, string? Address, string? UserName);

/// <summary>The settings file couldn't be written; the message is the file system's reason.</summary>
public sealed class ConnectionNotSavedException(Exception innerException) : Exception(innerException.Message, innerException);

/// <summary>What <see cref="ConnectionStore.Load"/> found: the Connection, or none and whether a damaged file is why.</summary>
public sealed record SavedConnection(Connection? Connection, bool Unreadable);
