namespace Jenkade.Core;

/// <summary>
/// Keeps secrets such as API tokens out of the settings files; the Windows Credential Manager on Windows.
/// Fails with a <see cref="CredentialStoreException"/> carrying the store's own reason.
/// </summary>
public interface ICredentialStore
{
    void Save(string key, string secret);

    string? Read(string key);
}

public class CredentialStoreException(string message, Exception? innerException = null) : Exception(message, innerException);
