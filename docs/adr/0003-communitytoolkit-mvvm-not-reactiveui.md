# CommunityToolkit.Mvvm instead of ReactiveUI for ViewModels

Jenkade's ViewModels use CommunityToolkit.Mvvm with plain `async`/`await`; polling (Job tree and Build history every 10 s, a live Console log every 1–2 s) is a `PeriodicTimer` on `TimeProvider` cancelled when the selection changes. ReactiveUI fits that streaming shape well and is not ruled out by Native AOT, but its current API is a fresh rewrite: simpler, widely documented code is worth more to us than Rx operators.

## Considered options

- **ReactiveUI 26** (with ReactiveUI.Avalonia 12.1.6 on Avalonia 12.1.4): a probe on 2026-10-22 covered `WhenAnyValue`, `Interval` + `Switch` polling, `ToProperty`, `ReactiveCommand`, `ReactiveWindow` and `OneWayBind`. It built a Native AOT `win-x64` exe with no trimming or AOT warnings, because version 26 drops System.Reactive for its own source-generated primitives. Its schedulers accept `TimeProvider`, so fake-time tests are possible, though delivery stays asynchronous. Rejected because version 26 renamed core types (`Signal`, `RxVoid`) and has overlapping APIs, ReactiveUI.Avalonia released 7 versions in under 3 months, most examples online describe the older System.Reactive API, and headless Avalonia tests with it were not verified.

## Consequences

- Revisit if polling logic in automatic refresh or the live Console log grows into combining several streams; plain System.Reactive or ReactiveUI in specific ViewModels would then need its own AOT probe.
