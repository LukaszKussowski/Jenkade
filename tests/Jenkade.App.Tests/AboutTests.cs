using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public class AboutTests
{
    [AvaloniaFact]
    public void About_window_shows_the_version_from_git_tags()
    {
        using var bindingErrors = BindingErrorLog.Capture();
        using var host = CompositionRoot.CreateHost();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Find<MenuItem>(mainWindow, "AboutMenuItem").RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var aboutWindow = Assert.IsType<AboutWindow>(Assert.Single(mainWindow.OwnedWindows));
        Screenshots.Save(aboutWindow, "about-window");
        // MinVer: 0.x until the MVP is complete; without a tag it's e.g. 0.1.0-alpha.0.12.
        Assert.Matches(@"^0\.\d+\.\d+(-[0-9A-Za-z.]+)?$", Find<TextBlock>(aboutWindow, "Version").Text);
        Assert.Empty(bindingErrors.Messages);
    }

    // Controls are looked up through the logical tree; see the prototype README.
    private static T Find<T>(ILogical root, string name) where T : Control =>
        root.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);
}
