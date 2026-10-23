using Jenkade.Core;
using Jenkade.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jenkade.App.Tests;

/// <summary>
/// The application with only its platform seams replaced: the fake Jenkins, an in-memory credential store and a
/// temporary settings folder. Hosts created from one instance share them, as runs on one machine do.
/// </summary>
public sealed class TestSeams : IDisposable
{
    private readonly TemporarySettingsFolder settings = new();

    public FakeJenkins Jenkins { get; } = new();

    public InMemoryCredentialStore Credentials { get; } = new();

    /// <param name="platformCredentialStore">Keeps the platform's own credential store instead of the in-memory one.</param>
    public IHost CreateHost(bool platformCredentialStore = false) => CompositionRoot.CreateHost(services =>
    {
        services.AddSingleton<HttpMessageHandler>(Jenkins).AddSingleton(settings.Folder);
        if (!platformCredentialStore)
            services.AddSingleton<ICredentialStore>(Credentials);
    });

    public void Dispose() => settings.Dispose();
}
