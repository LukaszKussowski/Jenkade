using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jenkade.Core;

namespace Jenkade.Presentation;

/// <summary>The Add-Connection form: saves a Connection only once Jenkins has verified it.</summary>
public sealed partial class AddConnectionViewModel(JenkinsClient jenkins, ConnectionStore connections) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
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

    /// <summary>Why the Connection wasn't saved, or null.</summary>
    [ObservableProperty]
    public partial string? Error { get; private set; }

    public event EventHandler<Connection>? Saved;

    /// <summary>The user closed the form: a verification still waiting for Jenkins saves nothing.</summary>
    public void Close() => SaveCommand.Cancel();

    private bool CanSave =>
        new[] { DisplayName, Address, UserName, ApiToken }.All(value => !string.IsNullOrWhiteSpace(value));

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!Connection.TryParseAddress(Address, out var address))
        {
            Error = Strings.InvalidAddress;
            return;
        }

        var (userName, apiToken) = (UserName.Trim(), ApiToken.Trim());
        ConnectionVerification verification;
        try
        {
            verification = await jenkins.VerifyAsync(address, userName, apiToken, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        Error = Explain(verification);
        if (Error is not null)
            return;

        Connection connection;
        try
        {
            connection = connections.Add(DisplayName.Trim(), address, userName, apiToken);
        }
        catch (CredentialStoreUnavailableException)
        {
            Error = Strings.NoCredentialStore;
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
