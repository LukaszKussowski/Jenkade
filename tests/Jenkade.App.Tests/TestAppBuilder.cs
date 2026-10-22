using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Jenkade.App.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Jenkade.App.Tests;

// ADR 0001: real Skia rendering for screenshots, and the bundled Inter font as the default
// because the container has no system fonts.
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .WithInterFont()
            .With(new FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" })
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
