using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public class AboutTests
{
    [AvaloniaTheory]
    [InlineData("en")]
    [InlineData("pl")]
    public void About_window_shows_the_version_from_git_tags(string language)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using var app = new TestSeams();
        using var host = app.CreateHost();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Ui.Click(Ui.Find<MenuItem>(mainWindow, "AboutMenuItem"));

        var aboutWindow = Assert.IsType<AboutWindow>(Assert.Single(mainWindow.OwnedWindows));
        Screenshots.Save(aboutWindow, $"about-window-{language}");
        Assert.Equal(MinVerVersion, Ui.Find<TextBlock>(aboutWindow, "Version").Text);
        Assert.Empty(bindingErrors.Messages);
    }

    private static string MinVerVersion =>
        typeof(AboutTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "MinVerVersion").Value!;
}
