using Jenkade.App.Views;
using Jenkade.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App;

// Resolves the owner window lazily: MainWindow -> MainWindowViewModel -> IDialogService would otherwise be a cycle.
internal sealed class DialogService(IServiceProvider services) : IDialogService
{
    public Task ShowAboutAsync()
    {
        var about = new AboutWindow(services.GetRequiredService<AboutViewModel>());
        return about.ShowDialog(services.GetRequiredService<MainWindow>());
    }
}
