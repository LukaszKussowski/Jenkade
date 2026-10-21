using Avalonia; using Avalonia.Controls; using Avalonia.Markup.Xaml; using Avalonia.Themes.Fluent; using Avalonia.Controls.ApplicationLifetimes;
namespace Probe;
public class App : Application {
  public override void Initialize() => Styles.Add(new FluentTheme());
  public override void OnFrameworkInitializationCompleted() {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d) d.MainWindow = new MainWindow();
    base.OnFrameworkInitializationCompleted();
  }
}
public class MainWindow : Window {
  public MainWindow() { Title = "Jenkade"; var b = new Button { Name = "Run", Content = "Run" }; var t = new TextBlock { Name = "Status", Text = "idle" };
    b.Click += (_, _) => t.Text = "running"; Content = new StackPanel { Children = { b, t } }; }
}
public static class Program {
  public static void Main(string[] a) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(a);
  public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont();
}
