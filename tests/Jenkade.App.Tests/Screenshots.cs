using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;

namespace Jenkade.App.Tests;

public static class Screenshots
{
    /// <summary>Saves the window's rendered frame as <c>screenshots/{name}.png</c> next to the test binaries.</summary>
    public static void Save(TopLevel window, string name)
    {
        var frame = window.CaptureRenderedFrame() ?? throw new InvalidOperationException($"No frame rendered for {name}.");
        var directory = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "screenshots"));
        frame.Save(Path.Combine(directory.FullName, $"{name}.png"), new PngBitmapEncoderOptions());
    }
}
