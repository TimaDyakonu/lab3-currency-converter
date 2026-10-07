# Core Service Contracts

These contracts describe the boundaries consumed by WPF and replaced by tests.
Exact type names may be refined during implementation, but behaviors and
failure semantics are mandatory.

## `IRatesProvider`

`GetLatestAsync(DateOnly requestedDate, CancellationToken cancellationToken)`
returns a `RateAcquisitionOutcome`.

- Requests the BNM XML for `requestedDate`.
- If the response is empty or unusable, tries preceding dates up to seven
  calendar days.
- Returns the first usable snapshot with `UsedFallback = true` when its date
  differs from `requestedDate`.
- Does not throw network, timeout, empty-response, or parse errors to the UI.
- Uses an injected cache abstraction to load a prior successful snapshot when
  all network candidates fail.
- Does not overwrite valid cache data with an invalid or partial response.

## `IBnmXmlParser`

`Parse(string xml)` returns a validated `RateSnapshot` or an explicit parse
failure.

- Reads the BNM date and currency entries.
- Requires positive `Nominal` and `Value`.
- Stores `Value / Nominal` as the per-unit MDL rate.
- Adds MDL with a rate of 1.
- Rejects empty, malformed, incomplete, and non-positive-rate documents.

## `IRateCache`

- `LoadAsync(CancellationToken)` returns the last valid snapshot or an explicit
  cache-miss/corrupt-cache outcome.
- `SaveAsync(RateSnapshot, CancellationToken)` atomically persists the latest
  successful snapshot as JSON under LocalApplicationData.
- Cache failures are non-fatal and never replace a usable network result.

## `IInputValidator`

`Validate(string amountText, string? sourceCode, string? targetCode)` returns
either a `ConversionRequest` or a user-facing validation error.

- Accepts comma and dot decimal separators.
- Rejects missing, alphabetic, zero, negative, and non-finite amounts.
- Requires both currency selections.

## `ICurrencyConverter`

`Convert(ConversionRequest request, RateSnapshot snapshot)` returns a
`ConversionResult`.

- Same source and target code returns the original amount.
- Otherwise applies `amount * sourceRate / targetRate`.
- Uses `decimal` and rounds the displayed amount to two decimal places.
- Reports missing currencies as an explicit calculation error.
