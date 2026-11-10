# .NET 11 (STS) for union types

Jenkade moved from .NET 10 (LTS) to .NET 11 (STS) on its release day, 2026-11-10, for C# 15's `union` types and `closed` class hierarchies: expected outcomes (a Connection verified or not, saved or not) become types whose every case a `switch` must handle, checked by the compiler (CS8509, an error under warnings-as-errors). Before, they were an enum with a `_ => throw` arm and exceptions that every caller had to remember to catch; one forgotten `catch` closed Jenkade (#22).

## Considered options

- **Stay on .NET 10 LTS with hand-written result records**: no exhaustiveness check, so a new case compiles and fails at runtime. Rejected: the check is the point.
- **A result library (OneOf, FluentResults)**: no exhaustiveness check either, and another dependency on the AOT path.

## Consequences

- STS: .NET 11 is supported until 2028-11-14, the same day as .NET 10 (release metadata, 2026-11-10). Jenkade moves to each new .NET release (.NET 12 in November 2027) instead of staying on an LTS for three years.
- The container image installs SDK 11 next to SDK 8 and 10: `mcr.microsoft.com/dotnet/sdk:11.0` images are Ubuntu 26.04 only, while the image is built on 24.04.
- Avalonia 12 and CommunityToolkit.Mvvm 8.4 are built for earlier frameworks and run on .NET 11 unchanged: the headless tests and the trimming/AOT analyzers pass on it. A Native AOT publish on Windows (ADR 0002) is still unverified until the release workflow (#16).
- Rule for outcome types: `CODING_STANDARDS.md`, "Outcomes and exceptions".
