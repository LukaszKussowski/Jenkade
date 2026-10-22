# Headless Avalonia tests in the container

`tests/Jenkade.App.Tests` loads real XAML without a display (ADR 0001). Each setup piece below fixes a specific failure; when a headless test breaks, match the error message here first.

| Piece | Failure without it |
|---|---|
| `global.json` → `"test": { "runner": "Microsoft.Testing.Platform" }` | `Testing with VSTest target is no longer supported ... on .NET 10 SDK` |
| `xunit.v3.mtp-v2` pinned to the version `Avalonia.Headless.XUnit` is built against (3.2.2 for Avalonia 12.1.4); no `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio` | `MissingMethodException` in `TestIntrospectionHelper` (xUnit 4.x), or zero tests discovered |
| `<OutputType>Exe</OutputType>` in the test project | `Could not find app host executable` |
| `SkiaSharp.NativeAssets.Linux.NoDependencies` + `SkiaSharp.NativeAssets.Linux` with `ExcludeAssets="all"` | `libfontconfig.so.1: cannot open shared object file` |
| `Avalonia.Fonts.Inter` + `.WithInterFont()` + `FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" }` in `TestAppBuilder` | `Default font family name can't be null or empty` (the container has no system fonts) |
| Controls found through the logical tree, not `FindControl` | `Could not find parent name scope` for controls outside the window's name scope |
| `frame.Save(path, new PngBitmapEncoderOptions())` | `CS0618: 'Bitmap.Save(string, int?)' is obsolete` (fails the build: warnings are errors) |

To upgrade Avalonia, check which xUnit version the new `Avalonia.Headless.XUnit` depends on (its `.nuspec` on nuget.org) and move `xunit.v3.mtp-v2` to exactly that version.

## Test helpers

- `BindingErrorLog.Capture()` installs an Avalonia log sink that collects binding warnings and errors; assert `Messages` is empty.
- `Screenshots.Save(window, name)` writes `screenshots/{name}.png` next to the test binaries. CI uploads them as the `screenshots` artifact.
- `new LanguageScope("pl")` switches the UI language for one test and restores it on dispose; views read their text from `Strings` when they load, so create the window inside the scope.
