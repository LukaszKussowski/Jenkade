using Jenkade.Core;

namespace Jenkade.Testing;

/// <summary>Stands in for the Windows Credential Manager; share one instance across a simulated restart.</summary>
public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, string> secrets = [];

    public void Save(string key, string secret) => secrets[key] = secret;

    public string? Read(string key) => secrets.GetValueOrDefault(key);
}
