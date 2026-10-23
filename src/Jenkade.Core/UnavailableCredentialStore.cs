namespace Jenkade.Core;

/// <summary>
/// Stands in where the platform has no supported credential store yet (anything but Windows): Jenkade still starts,
/// and only keeping or reading an API token fails.
/// </summary>
public sealed class UnavailableCredentialStore : ICredentialStore
{
    public void Save(string key, string secret) => throw new CredentialStoreUnavailableException();

    public string? Read(string key) => throw new CredentialStoreUnavailableException();
}

public sealed class CredentialStoreUnavailableException()
    : InvalidOperationException("This platform has no supported credential store for API tokens.");
