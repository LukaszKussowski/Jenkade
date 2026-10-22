using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var host = CompositionRoot.CreateHost();
            desktop.MainWindow = host.Services.GetRequiredService<MainWindow>();
            desktop.Exit += (_, _) => host.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
