# Native AOT releases, built on Windows in CI rather than in the container

Jenkade ships as Native AOT (faster startup, smaller package: about 38 MB instead of 83 MB self-contained), so the code is AOT-compatible from day one (`IsAotCompatible`, warnings as errors, source-generated JSON, compiled bindings). Release binaries are built by GitHub Actions on `windows-latest`, because .NET does not officially support Native AOT compilation from Linux to Windows. Day-to-day work and tests in the container use regular JIT builds.

## Considered options

- **AOT built in the container**: technically works (verified 2026-10-21 on .NET 10.0.13), but only through a workaround: `DisableUnsupportedError=true`, `IlcUseEnvironmentalTools=true`, `CppLinker=lld-link`, `/libpath:` to `/opt/xwin/{crt/lib,sdk/lib/ucrt,sdk/lib/um}/x86_64`, and removing the `/SOURCELINK` option, which `lld-link` does not support, from `LinkerArg`. The path is unsupported and may break with any .NET update. Kept as a fallback.
- **No AOT**: simpler, but slower startup and a package twice the size.
