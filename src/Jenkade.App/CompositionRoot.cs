using Jenkade.App.Views;
using Jenkade.Core;
using Jenkade.Presentation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jenkade.App;

public static class CompositionRoot
{
    /// <param name="replaceSeams">
    /// Registers test stand-ins for the platform seams after the real ones (the last registration wins):
    /// <see cref="HttpMessageHandler"/> (Jenkins), <see cref="ICredentialStore"/>, <see cref="SettingsFolder"/>, <see cref="TimeProvider"/>.
    /// </param>
    public static IHost CreateHost(Action<IServiceCollection>? replaceSeams = null)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        builder.Services.AddSingleton(ApplicationVersion.FromAssembly(typeof(CompositionRoot).Assembly));
        // No certificate validation callback: TLS trust is the operating system's (no way to ignore certificate errors).
        builder.Services.AddSingleton<HttpMessageHandler>(_ => new SocketsHttpHandler());
        builder.Services.AddSingleton<ICredentialStore>(_ => OperatingSystem.IsWindows()
            ? new WindowsCredentialManager()
            : throw new PlatformNotSupportedException("Jenkade stores API tokens in the Windows Credential Manager only."));
        builder.Services.AddSingleton(_ => SettingsFolder.ForCurrentUser());
        builder.Services.AddSingleton(TimeProvider.System);
        replaceSeams?.Invoke(builder.Services);

        builder.Services.AddSingleton<JenkinsClient>();
        builder.Services.AddSingleton<ConnectionStore>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<AboutViewModel>();
        builder.Services.AddTransient<AddConnectionViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        return builder.Build();
    }
}
