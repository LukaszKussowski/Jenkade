using System.Linq; using Avalonia.LogicalTree; using Avalonia; using Avalonia.Controls; using Avalonia.Headless; using Avalonia.Headless.XUnit; using Avalonia.Input; using Avalonia.Interactivity; using Xunit;
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
public class TestAppBuilder { public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Probe.App>().UseSkia().WithInterFont().With(new Avalonia.Media.FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" }).UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }); }
public class UiTests {
  [AvaloniaFact] public void ClickingRunChangesStatus() {
    var w = new Probe.MainWindow(); w.Show();
    var b = w.GetLogicalDescendants().OfType<Button>().Single(c => c.Name == "Run"); var t = w.GetLogicalDescendants().OfType<TextBlock>().Single(c => c.Name == "Status");
    b.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    Assert.Equal("running", t.Text);
    var frame = w.CaptureRenderedFrame(); Assert.NotNull(frame); frame!.Save("screenshot.png");
  }
}
