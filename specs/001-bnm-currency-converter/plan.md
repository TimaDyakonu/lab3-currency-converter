# Implementation Plan: BNM Currency Converter

**Branch**: `001-bnm-currency-converter` | **Date**: 2026-10-07 | **Spec**:
[spec.md](./spec.md)

**Input**: Feature specification from
`/specs/001-bnm-currency-converter/spec.md`

## Summary

Deliver a WPF desktop currency converter that loads official BNM XML rates,
parses nominal-based rates against MDL, converts with `decimal`, and remains
usable during network, publication-date, input, and cache failures. The
implementation uses a three-project solution: a UI-independent Core library,
an MVVM WPF application, and xUnit tests. `IRatesProvider` isolates network
access, while a JSON snapshot in LocalApplicationData provides restart-safe
fallback.

## Technical Context

**Language/Version**: C# / .NET 8 LTS

**Primary Dependencies**: WPF, MVVM presentation classes, `HttpClient`,
`System.Xml.Linq`, `System.Text.Json`, xUnit

**Storage**: One JSON rate snapshot in `%LOCALAPPDATA%` application storage

**Testing**: xUnit; `dotnet test` from repository root

**Target Platform**: Windows desktop with .NET 8 WPF

**Project Type**: Desktop application with class library and test project

**Performance Goals**: The UI remains responsive during rate retrieval; normal
conversion after rates are loaded completes from in-memory data without
noticeable delay.

**Constraints**: No console interface; network requests have an explicit
timeout; invalid, empty, malformed, and unavailable data never crashes the
application; rate fallback searches at most seven preceding calendar days;
results use `decimal` and display two decimal places.

**Scale/Scope**: One desktop window, one daily BNM snapshot, a bounded currency
list from the source, one active conversion at a time, and focused unit tests
for Core behavior.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

All gates pass:

- Desktop GUI only: WPF is the sole user-facing interface.
- Official source: rates come from BNM's date-based XML endpoint.
- Separation: parsing, calculation, validation, provider, and cache remain in
  `CurrencyConverter.Core`; WPF contains orchestration and presentation only.
- Testability: Core services depend on abstractions and are covered by xUnit.
- Resilience: network, empty response, malformed XML, invalid input, and cache
  failures produce explicit result/status states rather than exceptions
  escaping to the UI.
- Specification completeness: user-visible behavior, failure behavior, and
  validation commands are defined in the feature specification.

## Project Structure

### Documentation (this feature)

```text
specs/001-bnm-currency-converter/
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   └── core-services.md
├── quickstart.md
└── tasks.md
```

### Source Code (repository root)

```text
CurrencyConverter.sln
src/
├── CurrencyConverter.Core/
│   ├── Models/
│   ├── Parsing/
│   ├── Services/
│   ├── Validation/
│   └── Storage/
├── CurrencyConverter.App/
│   ├── Views/
│   ├── ViewModels/
│   └── Services/
└── CurrencyConverter.Tests/
    ├── Parsing/
    ├── Services/
    ├── Validation/
    └── Storage/
README.md
```

**Structure Decision**: Use a solution at the repository root with a strict
dependency direction: `CurrencyConverter.App` references `Core`, and
`CurrencyConverter.Tests` references `Core` (and only the UI assembly tests
required presentation behavior). `Core` has no WPF dependency. The application
uses MVVM binding and delegates all domain/data work to Core services.

## Complexity Tracking

No constitution violations or additional complexity exceptions are required.
