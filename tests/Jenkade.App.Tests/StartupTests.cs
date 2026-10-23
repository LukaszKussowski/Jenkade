using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public class StartupTests
{
    [AvaloniaTheory]
    [InlineData("en", "_Help")]
    [InlineData("pl", "_Pomoc")]
    public void Main_window_opens_in_the_UI_language_without_binding_errors(string language, string helpMenu)
    {
        using var _ = new LanguageScope(language);
        using var bindingErrors = BindingErrorLog.Capture();
        using var app = new TestSeams();
        using var host = app.CreateHost();

        var window = host.Services.GetRequiredService<MainWindow>();
        window.Show();
        Screenshots.Save(window, $"main-window-{language}");

        Assert.True(window.IsVisible);
        Assert.Equal("Jenkade", window.Title);
        Assert.Equal(helpMenu, window.GetLogicalDescendants().OfType<MenuItem>().Single(item => item.Name == "HelpMenuItem").Header);
        Assert.Empty(bindingErrors.Messages);
    }
}
