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

    public IHost CreateHost() => CompositionRoot.CreateHost(services => services
        .AddSingleton<HttpMessageHandler>(Jenkins)
        .AddSingleton<ICredentialStore>(Credentials)
        .AddSingleton(settings.Folder));

    public void Dispose() => settings.Dispose();
}
