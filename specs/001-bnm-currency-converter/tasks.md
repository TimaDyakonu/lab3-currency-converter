# Tasks: BNM Currency Converter

**Input**: Design documents from
`/specs/001-bnm-currency-converter/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md,
contracts/core-services.md

**Tests**: Required by the specification and constitution. Every listed
edge-case test is a separate xUnit task.

## Phase 1: Setup (Solution and Projects)

**Purpose**: Create a buildable .NET 8 solution with the three projects and
the dependency direction defined by the plan.

- [ ] T001 Create `CurrencyConverter.sln` and the `src/` project directories from the plan using .NET 8 project templates.
- [ ] T002 Create `src/CurrencyConverter.Core/CurrencyConverter.Core.csproj` as a .NET 8 class library with no WPF dependency.
- [ ] T003 Create `src/CurrencyConverter.App/CurrencyConverter.App.csproj` as a .NET 8 WPF executable targeting Windows and reference `CurrencyConverter.Core`.
- [ ] T004 Create `src/CurrencyConverter.Tests/CurrencyConverter.Tests.csproj` as a .NET 8 xUnit test project and reference `CurrencyConverter.Core`.
- [ ] T005 Add all three projects to `CurrencyConverter.sln`, remove template placeholder files, and verify `dotnet build` succeeds from the repository root.

## Phase 2: Foundational Core Contracts

**Purpose**: Establish shared result types, abstractions, and deterministic
failure semantics before feature-specific implementation.

- [ ] T006 [P] Create the domain models `Currency`, `ExchangeRate`, `RateSnapshot`, `ConversionRequest`, `ConversionResult`, and `RateAcquisitionOutcome` in `src/CurrencyConverter.Core/Models/`, preserving the constraints that currency codes are non-empty and unique, numeric rates and nominal values are positive `decimal`s, and every snapshot includes MDL at rate 1.
- [ ] T007 [P] Define `IRatesProvider`, `IBnmXmlParser`, `IRateCache`, `IInputValidator`, and `ICurrencyConverter` in `src/CurrencyConverter.Core/Services/` and `src/CurrencyConverter.Core/Parsing/` according to `contracts/core-services.md`.
- [ ] T008 [P] Add explicit Core result/error types in `src/CurrencyConverter.Core/Models/` so transport failures, unavailable rates, parse failures, cache misses, and validation failures cannot become empty success-shaped values.
- [ ] T009 Create shared date, decimal, and source constants in `src/CurrencyConverter.Core/Services/` for `dd.MM.yyyy`, invariant BNM decimal parsing, the BNM URL template, and `National Bank of Moldova (BNM)`.
- [ ] T010 Add a Core test fixture builder with valid MDL, unit-nominal, and non-unit-nominal snapshots in `src/CurrencyConverter.Tests/Fixtures/` for reuse by all unit tests.

## Phase 3: User Story 1 - Convert an Amount (Priority: P1)

**Goal**: Load and normalize official BNM XML rates, calculate direct and
MDL-mediated conversions, and expose a reliable conversion service.

**Independent Test**: Supply a deterministic XML fixture and a
`ConversionRequest`; verify nominal handling, MDL conversion, reverse
conversion, same-currency conversion, and two-decimal output without any WPF
or network dependency.

### Core implementation

- [ ] T011 [US1] Implement `BnmXmlParser` in `src/CurrencyConverter.Core/Parsing/BnmXmlParser.cs` using `System.Xml.Linq`; parse `ValCurs/@Date` and each `Valute` `CharCode`, `Name`, `Nominal`, and `Value` with invariant culture, normalize `Value / Nominal`, add MDL at 1, and reject empty, malformed, incomplete, duplicate, or non-positive records.
- [ ] T012 [US1] Implement `CurrencyConverter` in `src/CurrencyConverter.Core/Services/CurrencyConverter.cs` using `decimal`; calculate `amount * sourceRate / targetRate`, preserve full precision until display rounding, return the original amount for identical currencies, and include rate date and BNM source metadata.
- [ ] T013 [US1] Implement `InputValidator` in `src/CurrencyConverter.Core/Validation/InputValidator.cs` to require both currency codes and accept comma or dot decimal separators while rejecting empty, alphabetic, zero, negative, overflow, and non-finite amounts.
- [ ] T014 [US1] Add parser unit tests in `src/CurrencyConverter.Tests/Parsing/BnmXmlParserTests.cs` for valid XML, MDL insertion, invariant decimal parsing, `Nominal` normalization, malformed XML, empty XML, missing fields, duplicate codes, and non-positive rates.
- [ ] T015 [US1] Add conversion unit tests in `src/CurrencyConverter.Tests/Services/CurrencyConverterTests.cs` for direct, reverse, foreign-to-foreign-through-MDL, identical currency, and two-decimal display results.
- [ ] T016 [US1] Add input validation unit tests in `src/CurrencyConverter.Tests/Validation/InputValidatorTests.cs` for positive values, comma separator, dot separator, empty input, letters, zero, negative values, overflow, and missing selectors.

### WPF conversion interface

- [ ] T017 [US1] Create `MainWindow.xaml` in `src/CurrencyConverter.App/Views/` with amount input, source and target currency selectors, Convert button, result area, rate date, source label, and status/fallback message bindings; do not put calculation or XML logic in code-behind.
- [ ] T018 [US1] Create `MainViewModel.cs` in `src/CurrencyConverter.App/ViewModels/` with bindable amount, selected currencies, currency list, result, rate date, source, status, and busy state; disable Convert until amount and both selectors are populated.
- [ ] T019 [US1] Create `ConvertCommand` and application composition in `src/CurrencyConverter.App/Services/` so the ViewModel calls Core interfaces asynchronously and never blocks the UI thread.

**Checkpoint**: With a deterministic provider/cache, the WPF view can display a
successful conversion and all core conversion tests pass.

## Phase 4: User Story 2 - Rate Freshness and Fallbacks (Priority: P1)

**Goal**: Retrieve current BNM rates, search previous dates for empty
publication days, and persist the latest successful snapshot for offline use.

**Independent Test**: Inject an HTTP handler and cache double, simulate
successful XML, empty/404 dates, transport failure, and restart, and verify
explicit fallback metadata without a thrown UI exception.

### Core implementation

- [ ] T020 [US2] Implement `BnmRatesProvider` in `src/CurrencyConverter.Core/Services/BnmRatesProvider.cs` with an injected long-lived `HttpClient`, finite timeout, cancellation support, date-formatted BNM URI generation, and explicit handling for non-success responses, timeouts, empty bodies, and parse failures.
- [ ] T021 [US2] Add bounded previous-date search to `BnmRatesProvider` in `src/CurrencyConverter.Core/Services/BnmRatesProvider.cs`; after an empty or unusable response, try each preceding calendar date for at most seven days, use the returned `ValCurs/@Date`, and set `UsedFallback` when it differs from the requested date.
- [ ] T022 [US2] Implement `JsonRateCache` in `src/CurrencyConverter.Core/Storage/JsonRateCache.cs` using `System.Text.Json`, `%LOCALAPPDATA%` application storage, validated snapshot metadata, directory creation, temporary-file replacement, and non-fatal cache read/write outcomes.
- [ ] T023 [US2] Integrate `IRateCache` into `BnmRatesProvider` so only complete successful snapshots are saved, valid cached data is offered after network/search failure, corrupt or empty cache data is rejected, and the UI receives an explicit cached-date message.
- [ ] T024 [P] [US2] Add a provider test double and HTTP handler in `src/CurrencyConverter.Tests/Fixtures/` that maps requested dates to deterministic XML, empty bodies, HTTP errors, and thrown timeout/network exceptions.
- [ ] T025 [US2] Add the required no-network unit test in `src/CurrencyConverter.Tests/Services/BnmRatesProviderNetworkTests.cs`; simulate a transport failure and verify no exception escapes, cached rates are offered when present, and the cached rate date is exposed.
- [ ] T026 [US2] Add the required empty-weekend/holiday unit test in `src/CurrencyConverter.Tests/Services/BnmRatesProviderFallbackTests.cs`; return empty or 404 responses for requested and preceding dates, verify search stops after seven days, and verify the first usable snapshot is marked with its actual date.
- [ ] T027 [US2] Add the required cache-after-restart unit test in `src/CurrencyConverter.Tests/Storage/JsonRateCachePersistenceTests.cs`; save through one cache instance, create a second instance pointing at the same temporary LocalApplicationData path, load the snapshot, and verify rates/date/source survive.
- [ ] T028 [US2] Add cache corruption and write-failure tests in `src/CurrencyConverter.Tests/Storage/JsonRateCacheTests.cs` to verify invalid JSON, missing fields, and filesystem errors produce explicit non-fatal outcomes without replacing a usable network snapshot.

### WPF freshness and fallback interface

- [ ] T029 [US2] Wire the production `BnmRatesProvider`, `JsonRateCache`, parser, validator, and converter in `src/CurrencyConverter.App/Services/AppComposition.cs` without leaking infrastructure into XAML.
- [ ] T030 [US2] Update `MainViewModel.cs` and `MainWindow.xaml` so current, previous-date, cached, unavailable, timeout, and source-date statuses are always visible and distinguish fallback results from current BNM results.

**Checkpoint**: Network failure, empty publication-day responses, and restart
with a persisted cache all remain non-crashing and user-transparent.

## Phase 5: User Story 3 - Correct Invalid Input (Priority: P1)

**Goal**: Prevent invalid calculations and clearly communicate validation
errors while preserving the valid same-currency flow.

**Independent Test**: Exercise the UI command with empty, alphabetic, zero,
negative, comma, dot, missing-selector, and same-currency inputs; verify
invalid requests do not reach the provider and valid same-currency requests
return the input amount.

- [ ] T031 [US3] Add the required invalid-input unit test in `src/CurrencyConverter.Tests/Validation/InputValidationEdgeCaseTests.cs`; verify empty, alphabetic, zero, and negative input returns a clear error, does not call `IRatesProvider`, and does not produce a conversion result.
- [ ] T032 [US3] Add the required identical-currencies unit test in `src/CurrencyConverter.Tests/Services/IdenticalCurrencyConversionTests.cs`; verify source and target with the same code return the entered amount without a provider error and preserve rate metadata.
- [ ] T033 [US3] Add ViewModel command tests in `src/CurrencyConverter.Tests/App/MainViewModelTests.cs` for Convert button enabled state, validation status, comma/dot input, no provider call for invalid input, and visible result/source/date bindings for valid input.
- [ ] T034 [US3] Refine `MainViewModel.cs` and `MainWindow.xaml` validation presentation so invalid text, zero, negative values, and missing selectors remain in the window with a corrective message and never terminate the application.

**Checkpoint**: All five requested boundary behaviors have dedicated
implementation coverage and separate unit-test tasks.

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Verify the complete solution and publish runnable repository
instructions. This phase ends with README as the final implementation task.

- [ ] T035 [P] Add solution-level test helpers and deterministic culture setup in `src/CurrencyConverter.Tests/` so tests are independent of the machine locale and never call the live BNM service.
- [ ] T036 Run `dotnet build` from the repository root, fix all compiler/analyzer errors in the affected project files, and confirm the WPF executable and test assembly are produced without manual steps.
- [ ] T037 Run `dotnet test` from the repository root and confirm the full suite covers the five required boundary cases plus nominal, MDL, parsing, and UI-state scenarios.
- [ ] T038 Execute every validation flow in `specs/001-bnm-currency-converter/quickstart.md`, confirm rate date/source visibility and non-crashing failure behavior, and correct any discrepancy in source or test files.
- [ ] T039 Add `README.md` at the repository root as the final task, documenting prerequisites, `dotnet build`, `dotnet test`, the WPF run command, expected application behavior, and offline/cache validation without requiring console interaction from end users.

## Dependencies and Execution Order

### Phase Dependencies

- Phase 1 is independent and must complete before Phase 2.
- Phase 2 blocks all user-story work because it defines shared models,
  interfaces, result semantics, constants, and fixtures.
- Phase 3 can start after Phase 2 and is the MVP conversion slice.
- Phase 4 depends on Phase 3 Core models/parser and adds network/cache
  behavior; its WPF status work depends on the Phase 3 ViewModel.
- Phase 5 depends on the shared validator and ViewModel from Phases 2–4.
- Phase 6 depends on all requested user stories; T039 is intentionally last.

### User Story Dependencies

- **US1**: Starts after Phase 2; no other story dependency.
- **US2**: Uses US1 models/parser and application composition.
- **US3**: Uses US1 validator/converter and US2-composed ViewModel.

### Parallel Opportunities

- T006–T009 can run in parallel after the project scaffold exists.
- T014, T015, and T016 can run in parallel after their corresponding Core
  contracts are present.
- T024 can run in parallel with T022; T025–T028 can begin after provider/cache
  seams exist and can run in parallel with WPF status wiring.
- T031 and T032 can run in parallel; T033 follows the ViewModel test seam.
- T035 can run in parallel with final manual validation, but T036–T039 remain
  ordered and T039 must be last.

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 and stop at its checkpoint.
3. Validate `dotnet build`, Core tests, and deterministic WPF conversion.
4. Add Phase 4 offline/fallback/cache behavior.
5. Add Phase 5 invalid-input and same-currency safeguards.
6. Run Phase 6, with README last.

### Independent Story Test Criteria

- **US1**: A valid fixture converts direct, reverse, foreign-through-MDL,
  nominal-adjusted, and same-currency amounts with two-decimal display.
- **US2**: Injected network failures and empty publication responses never crash,
  search no more than seven prior days, and expose actual current/cached dates;
  a second cache instance loads the saved snapshot.
- **US3**: Invalid amount or missing selector prevents provider invocation and
  shows a corrective message; identical currencies return the original amount.

All tasks use the required checklist format, sequential IDs, explicit story
labels for story phases, `[P]` only where work can proceed independently, and
concrete file paths.
