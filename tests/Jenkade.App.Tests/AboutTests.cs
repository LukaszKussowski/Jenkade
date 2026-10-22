using System.Reflection;
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
        Assert.Equal(MinVerVersion, Find<TextBlock>(aboutWindow, "Version").Text);
        Assert.Empty(bindingErrors.Messages);
    }

    private static string MinVerVersion =>
        typeof(AboutTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "MinVerVersion").Value!;

    // FindControl can't see controls outside the window's name scope ("Could not find parent name scope");
    // the logical tree finds any control.
    private static T Find<T>(ILogical root, string name) where T : Control =>
        root.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);
}
