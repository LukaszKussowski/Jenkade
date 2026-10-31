using Jenkade.Core;

namespace Jenkade.Testing;

/// <summary>Stands in for the Windows Credential Manager; share one instance across a simulated restart.</summary>
public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, string> secrets = [];

    /// <summary>Fails every save and read, as a Credential Manager refusing a too-long token does.</summary>
    public bool Failing { get; set; }

    public void Save(string key, string secret)
    {
        ThrowIfFailing();
        secrets[key] = secret;
    }

    public string? Read(string key)
    {
        ThrowIfFailing();
        return secrets.GetValueOrDefault(key);
    }

    private void ThrowIfFailing()
    {
        if (Failing)
            throw new CredentialStoreException("The parameter is incorrect.");
    }
}
