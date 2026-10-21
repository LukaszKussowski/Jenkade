# Prototype: Avalonia headless UI tests and Native AOT from the Linux container

Throwaway code from the design session (2026-10-21). It answered two questions and is kept as the primary source behind ADR 0001 and ADR 0002. Don't build on it; copy the decision-rich parts.

## Headless UI test in the container (ADR 0001)

`dotnet test --project tests` passes in the dev container: it opens a window, clicks a button, asserts the text changed and saves `screenshot.png`. Each piece below fixed a concrete failure:

| Piece | Failure without it |
|---|---|
| `global.json` → `"test": { "runner": "Microsoft.Testing.Platform" }` | `Testing with VSTest target is no longer supported ... on .NET 10 SDK` |
| `xunit.v3.mtp-v2` pinned to `3.2.2`, the version `Avalonia.Headless.XUnit` 12.1.4 is built against; no `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio` | `MissingMethodException` in `TestIntrospectionHelper` (xUnit 4.x), or zero tests discovered |
| `<OutputType>Exe</OutputType>` in the test project | `Could not find app host executable` |
| `SkiaSharp.NativeAssets.Linux.NoDependencies` + `SkiaSharp.NativeAssets.Linux` with `ExcludeAssets="all"` | `libfontconfig.so.1: cannot open shared object file` |
| `Avalonia.Fonts.Inter` + `.WithInterFont()` + `FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" }` in the test app builder | `Default font family name can't be null or empty` (the container has no system fonts) |
| Controls found via the logical tree, not `FindControl` | `Could not find parent name scope` for controls created in code (XAML-defined names don't have this problem) |

Versions: .NET SDK 10.0.402, Avalonia 12.1.4, SkiaSharp 3.119.4.

## Native AOT `win-x64` from Linux (ADR 0002)

`dotnet publish app -c Release -r win-x64 -p:PublishAot=true` succeeds in the container with `Directory.Build.targets`, which swaps the MSVC linker for `lld-link` with the `/opt/xwin` libraries and drops `/SOURCELINK`. The resulting `App.exe` (19 MB plus Skia, HarfBuzz and ANGLE DLLs) ran correctly on Windows. Releases still build on `windows-latest`; this is the fallback.
