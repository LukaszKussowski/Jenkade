using Avalonia.Headless.XUnit;
using Jenkade.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Jenkade.App.Tests;

public class StartupTests
{
    [AvaloniaFact]
    public void Main_window_opens_without_binding_errors()
    {
        using var bindingErrors = BindingErrorLog.Capture();
        using var host = CompositionRoot.CreateHost();

        var window = host.Services.GetRequiredService<MainWindow>();
        window.Show();
        Screenshots.Save(window, "main-window");

        Assert.True(window.IsVisible);
        Assert.Equal("Jenkade", window.Title);
        Assert.Empty(bindingErrors.Messages);
    }
}
