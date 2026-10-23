using System.Runtime.Versioning;

namespace Jenkade.App.Tests;

// Runs on the Windows CI job; skipped elsewhere. Uses the real Credential Manager of the account running the tests.
// SupportedOSPlatform satisfies the platform analyzer (CA1416), which doesn't see Assert.SkipUnless as a guard.
[SupportedOSPlatform("windows")]
public class WindowsCredentialManagerTests
{
    // One fixed key, overwritten on every run, so test runs don't pile up entries in the Credential Manager.
    private const string Key = "Jenkade:Tests:WindowsCredentialManager";

    [Fact]
    public void Keeps_a_secret_and_replaces_it_when_saved_again()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The Windows Credential Manager exists only on Windows.");
        var credentials = new WindowsCredentialManager();

        credentials.Save(Key, "11a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7");
        credentials.Save(Key, "99f8e7d6c5b4a3928170f6e5d4c3b2a1f0");

        Assert.Equal("99f8e7d6c5b4a3928170f6e5d4c3b2a1f0", credentials.Read(Key));
    }

    [Fact]
    public void Has_no_secret_for_a_key_never_saved()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "The Windows Credential Manager exists only on Windows.");

        Assert.Null(new WindowsCredentialManager().Read($"Jenkade:Tests:{Guid.NewGuid()}"));
    }
}
