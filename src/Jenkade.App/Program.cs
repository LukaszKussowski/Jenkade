using System.Globalization;
using Avalonia;
using Jenkade.Presentation;

namespace Jenkade.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Before any window loads its text: the language is fixed for the whole run.
        UiLanguage.Apply(UiLanguage.FromOperatingSystem(CultureInfo.CurrentUICulture));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Also used by the Avalonia previewer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
