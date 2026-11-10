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
    /// The saved Connection; <see cref="SavedConnection.None"/> before the user has set one up, and
    /// <see cref="SavedConnection.Unreadable"/> when <c>connection.json</c> can't be read or doesn't describe a
    /// Connection. Saving one replaces the file.
    /// </summary>
    public SavedConnection Load()
    {
        if (!File.Exists(SettingsFile))
            return new SavedConnection.None();
        try
        {
            using var stream = File.OpenRead(SettingsFile);
            // Read field by field: the deserializer leaves missing fields null instead of failing.
            if (JsonSerializer.Deserialize(stream, ConnectionJson.Default.ConnectionFile) is { Address: { } address, UserName: { } userName } file
                && Connection.TryParseAddress(address, out var parsedAddress) && !string.IsNullOrWhiteSpace(userName))
                return new Connection(file.DisplayName ?? "", parsedAddress, userName);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
        }
        return new SavedConnection.Unreadable();
    }

    /// <summary>
    /// Saves the Connection and its API token in place of the ones saved before; when that fails, the ones saved before
    /// stay.
    /// </summary>
    public ConnectionSaving Save(Connection connection, string apiToken)
    {
        string? previousApiToken;
        try
        {
            previousApiToken = credentials.Read(CredentialKey);
            credentials.Save(CredentialKey, apiToken);
        }
        catch (CredentialStoreUnavailableException)
        {
            return new NoCredentialStore();
        }
        catch (CredentialStoreException exception)
        {
            return new CredentialStoreFailed(exception.Message);
        }

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
            return new ConnectionSaving.SettingsFileNotWritten(exception.Message);
        }
        return new ConnectionSaving.Saved();
    }

    public SavedApiToken ApiToken()
    {
        try
        {
            return credentials.Read(CredentialKey) is { } apiToken ? apiToken : new SavedApiToken.None();
        }
        catch (CredentialStoreUnavailableException)
        {
            return new NoCredentialStore();
        }
        catch (CredentialStoreException exception)
        {
            return new CredentialStoreFailed(exception.Message);
        }
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(ConnectionFile))]
internal sealed partial class ConnectionJson : JsonSerializerContext;

/// <summary>The contents of <c>connection.json</c>, each field possibly missing.</summary>
internal sealed record ConnectionFile(string? DisplayName, string? Address, string? UserName);

/// <summary>What <see cref="ConnectionStore.Load"/> found.</summary>
public union SavedConnection(Connection, SavedConnection.None, SavedConnection.Unreadable)
{
    /// <summary>The user hasn't set up a Connection yet.</summary>
    public sealed record None;

    /// <summary><c>connection.json</c> is damaged or can't be read: the user sets the Connection up again.</summary>
    public sealed record Unreadable;
}

/// <summary>What <see cref="ConnectionStore.Save"/> did.</summary>
public union ConnectionSaving(ConnectionSaving.Saved, NoCredentialStore, CredentialStoreFailed, ConnectionSaving.SettingsFileNotWritten)
{
    public sealed record Saved;

    /// <summary>The settings file couldn't be written; the reason is the file system's.</summary>
    public sealed record SettingsFileNotWritten(string Reason);
}

/// <summary>The API token saved with the Connection, as <see cref="ConnectionStore.ApiToken"/> read it.</summary>
public union SavedApiToken(string, SavedApiToken.None, NoCredentialStore, CredentialStoreFailed)
{
    public sealed record None;
}

/// <summary>The platform has no supported credential store (anything but Windows, for now).</summary>
public sealed record NoCredentialStore;

/// <summary>The credential store refused; the reason is its own, e.g. in the Windows display language.</summary>
public sealed record CredentialStoreFailed(string Reason);
