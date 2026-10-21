# Avalonia instead of WPF or WinUI 3: container-testable and cross-platform UI

Jenkade targets Windows first, but it is developed in a Linux container that cannot run any `.exe` or show a window. We chose Avalonia 12 with the Fluent theme because its headless tests (`Avalonia.Headless.XUnit`) load real XAML, exercise bindings and render screenshots without a display, in the same container where the code is written. Logic and ViewModels live in UI-free `net10.0` libraries. Avalonia is also cross-platform, which keeps Linux and macOS releases open without a rewrite; WPF and WinUI 3 are Windows-only.

## Considered options

- **WPF**: builds in the container (`EnableWindowsTargeting`), but any test touching WPF needs `Microsoft.WindowsDesktop.App`, which does not exist on Linux. UI tests would only be possible with FlaUI on Windows (GitHub Actions or Windows Sandbox), outside the container feedback loop.
- **WinUI 3**: its XAML compiler runs only on Windows, so the app would not build in the container at all.

## Consequences

- Headless tests are not Windows: they won't catch platform-specific issues such as DPI handling or shell integration.
- Credential storage sits behind an interface: Windows Credential Manager now, other OS equivalents later.
- Headless testing in the container requires: xUnit v3 at the version `Avalonia.Headless.XUnit` was built against, Microsoft.Testing.Platform enabled in `global.json`, dependency-free native Skia (`SkiaSharp.NativeAssets.Linux.NoDependencies`, with the regular package excluded), and a bundled font (`Avalonia.Fonts.Inter`) set as the default font family.
