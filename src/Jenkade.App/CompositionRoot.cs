using Jenkade.App.Views;
using Jenkade.Core;
using Jenkade.Presentation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jenkade.App;

public static class CompositionRoot
{
    public static IHost CreateHost()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        builder.Services.AddSingleton(ApplicationVersion.FromAssembly(typeof(CompositionRoot).Assembly));
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<AboutViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        return builder.Build();
    }
}
