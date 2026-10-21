# Native AOT releases, built on Windows in CI rather than in the container

Jenkade ships as Native AOT (faster startup, smaller package: about 38 MB instead of 83 MB self-contained), so the code is AOT-compatible from day one (`IsAotCompatible`, warnings as errors, source-generated JSON, compiled bindings). Release binaries are built by GitHub Actions on `windows-latest`, because .NET does not officially support Native AOT compilation from Linux to Windows. Day-to-day work and tests in the container use regular JIT builds.

## Considered options

- **AOT built in the container**: works: on 2026-10-21 (.NET 10.0.13, Avalonia 12.1.4) a probe app was cross-compiled in the container and ran correctly on Windows. It needs a workaround, though: `DisableUnsupportedError=true`, `IlcUseEnvironmentalTools=true`, `CppLinker=lld-link`, `/libpath:` to `/opt/xwin/{crt/lib,sdk/lib/ucrt,sdk/lib/um}/x86_64`, and removing the `/SOURCELINK` option, which `lld-link` does not support, from `LinkerArg`. The path is unsupported and may break with any .NET update. Kept as a fallback.
- **No AOT**: simpler, but slower startup and a package twice the size.
