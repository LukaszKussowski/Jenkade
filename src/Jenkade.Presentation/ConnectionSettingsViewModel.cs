using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jenkade.Core;

namespace Jenkade.Presentation;

/// <summary>
/// The Connection settings form: sets up the one Connection, or replaces it (ADR 0005), saving only what Jenkins has
/// verified. Opens with the saved Connection; an empty API token field keeps the saved token.
/// </summary>
public sealed partial class ConnectionSettingsViewModel : ObservableObject
{
    private readonly JenkinsClient jenkins;
    private readonly ConnectionStore connections;
    private readonly bool hasSavedConnection;

    public ConnectionSettingsViewModel(JenkinsClient jenkins, ConnectionStore connections)
    {
        (this.jenkins, this.connections) = (jenkins, connections);
        if (connections.Load().Connection is { } saved)
        {
            hasSavedConnection = true;
            (DisplayName, Address, UserName) = (saved.DisplayName, saved.Address.ToString(), saved.UserName);
        }
    }

    /// <summary>Optional label; the main window names a Connection without one by its address.</summary>
    [ObservableProperty]
    public partial string DisplayName { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string Address { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string UserName { get; set; } = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial string ApiToken { get; set; } = "";

    /// <summary>Tells the user an empty API token field keeps the saved token.</summary>
    public string ApiTokenPlaceholder => hasSavedConnection ? Strings.KeepSavedApiToken : "";

    /// <summary>Why the Connection wasn't saved, or null.</summary>
    [ObservableProperty]
    public partial string? Error { get; private set; }

    public event EventHandler<Connection>? Saved;

    /// <summary>The user closed the form: a verification still waiting for Jenkins saves nothing.</summary>
    public void Close() => SaveCommand.Cancel();

    private bool CanSave =>
        !string.IsNullOrWhiteSpace(Address) && !string.IsNullOrWhiteSpace(UserName)
        && (hasSavedConnection || !string.IsNullOrWhiteSpace(ApiToken));

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!Connection.TryParseAddress(Address, out var address))
        {
            Error = Strings.InvalidAddress;
            return;
        }

        var connection = new Connection(DisplayName.Trim(), address, UserName.Trim());
        try
        {
            var apiToken = string.IsNullOrWhiteSpace(ApiToken) ? connections.ApiToken() ?? "" : ApiToken.Trim();
            var verification = await jenkins.VerifyAsync(address, connection.UserName, apiToken, cancellationToken);
            Error = Explain(verification);
            if (Error is not null)
                return;
            connections.Save(connection, apiToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (CredentialStoreUnavailableException)
        {
            Error = Strings.NoCredentialStore;
            return;
        }
        catch (Exception exception) when (exception is CredentialStoreException or ConnectionNotSavedException)
        {
            Error = string.Format(CultureInfo.CurrentCulture, Strings.ConnectionNotSaved, exception.Message);
            return;
        }

        Saved?.Invoke(this, connection);
    }

    private static string? Explain(ConnectionVerification verification) => verification switch
    {
        ConnectionVerification.Verified => null,
        ConnectionVerification.InvalidCredentials => Strings.VerificationInvalidCredentials,
        ConnectionVerification.NoPermission => Strings.VerificationNoPermission,
        ConnectionVerification.Unreachable => Strings.VerificationUnreachable,
        ConnectionVerification.UntrustedCertificate => Strings.VerificationUntrustedCertificate,
        ConnectionVerification.NotJenkins => Strings.VerificationNotJenkins,
        ConnectionVerification.Redirected => Strings.VerificationRedirected,
        ConnectionVerification.UnexpectedResponse => Strings.VerificationUnexpectedResponse,
        _ => throw new ArgumentOutOfRangeException(nameof(verification), verification, null),
    };
}
