# Coding standards

Judgement rules a reviewer applies to every change. Rules a tool can check live in the tool, listed at the end.

## Module boundaries

- **Core**: the Jenkins API client and the domain model. UI-free: no Avalonia, no ViewModels.
- **Presentation**: ViewModels on CommunityToolkit.Mvvm (ADR 0003). UI-free; depends on Core only. Windows and dialogs are reached through interfaces the App implements, like `IDialogService`.
- **App**: Avalonia views, the composition root and platform adapters (credential store). Behaviour that can be tested without XAML belongs in Presentation or Core.
- Core and Presentation stay platform-neutral: anything Windows-specific sits behind an interface implemented in App (ADR 0001).

## Domain language

Types, members, test names and UI text use the terms in `GLOSSARY.md`; its _Avoid_ words don't name domain concepts. Polish UI wording follows the glossary's _Polish UI_ entries. Data from Jenkins (Job names, Build parameters, Console log, server messages) is shown as Jenkins provides it, never translated.

## Tests

- Test at the highest seam: what the user sees, or what Jenkins receives.
- Replace only the agreed seams: Jenkins over HTTP (a fake `HttpMessageHandler` serving recorded responses), the credential store, the settings folder and `TimeProvider`. Everything between them is real.
- Assert on behaviour, not on private members, internal collections or call counts.
- Expected values come from an independent source (a literal, the spec, MinVer), never recomputed the way the code computes them.
- Headless UI tests assert there are no binding errors and save a screenshot (`docs/headless-testing.md`).

## Native AOT

- No reflection-based serialization or binding: `System.Text.Json` source generation, compiled XAML bindings with `x:DataType` (ADR 0002).
- A suppressed trimming or AOT warning carries a comment saying why it is safe.

## Time

Polling and timeouts take the injected `TimeProvider` (e.g. `PeriodicTimer` on it, ADR 0003); Core and Presentation don't read the clock or delay on real time directly.

## Enforced by tooling

- All warnings as errors, including trimming/AOT analyzers and Avalonia's XAML compiler (AVLN): `Directory.Build.props`.
- Polish text only in `*.pl.resx` and `GLOSSARY.md`: `scripts/check-polish-text.py`.
- No Claude attribution in commits or PR descriptions: `.github/workflows/conventions.yml`.
