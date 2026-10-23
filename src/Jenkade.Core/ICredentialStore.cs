namespace Jenkade.Core;

/// <summary>Keeps secrets such as API tokens out of the settings files; the Windows Credential Manager on Windows.</summary>
public interface ICredentialStore
{
    void Save(string key, string secret);

    string? Read(string key);
}
