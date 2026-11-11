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
            return Read(JsonSerializer.Deserialize(stream, ConnectionJson.Default.ConnectionFile));
        }
        catch (JsonException exception)
        {
            // LineNumber counts from 0, and is missing when the file is empty.
            return Unreadable(new SettingsFileFault.InvalidJson((exception.LineNumber ?? 0) + 1));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Unreadable(new SettingsFileFault.CannotOpen(exception.Message));
        }
    }

    // Field by field: the deserializer leaves missing fields null instead of failing.
    private SavedConnection Read(ConnectionFile? file)
    {
        if (string.IsNullOrWhiteSpace(file?.Address))
            return Unreadable(new SettingsFileFault.MissingAddress());
        if (!Connection.TryParseAddress(file.Address, out var address))
            return Unreadable(new SettingsFileFault.InvalidAddress(file.Address));
        if (string.IsNullOrWhiteSpace(file.UserName))
            return Unreadable(new SettingsFileFault.MissingUserName());
        return new Connection(file.DisplayName ?? "", address, file.UserName);
    }

    private SavedConnection.Unreadable Unreadable(SettingsFileFault fault) => new(SettingsFile, fault);

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

    /// <summary>The settings file can't be read as a Connection: the user sets the Connection up again.</summary>
    public sealed record Unreadable(string File, SettingsFileFault Fault);
}

/// <summary>Why <c>connection.json</c> can't be read as a Connection.</summary>
public closed record SettingsFileFault
{
    /// <summary>The file can't be opened, e.g. another program holds it or permissions forbid it.</summary>
    public sealed record CannotOpen(string SystemReason) : SettingsFileFault;

    /// <summary>The file isn't valid JSON from <see cref="Line"/> (counted from 1) on, e.g. it was cut short.</summary>
    public sealed record InvalidJson(long Line) : SettingsFileFault;

    public sealed record MissingAddress : SettingsFileFault;

    /// <summary>The address isn't an absolute http or https address.</summary>
    public sealed record InvalidAddress(string Address) : SettingsFileFault;

    public sealed record MissingUserName : SettingsFileFault;
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
