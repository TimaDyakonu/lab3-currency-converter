# Data Model: BNM Currency Converter

## Currency

- `Code`: official currency code; MDL is always available.
- `Name`: display name from the BNM response when provided.

**Validation**: `Code` is non-empty and unique within a snapshot.

## ExchangeRate

- `CurrencyCode`: `Currency.Code`.
- `Nominal`: positive decimal quantity quoted by BNM.
- `Value`: positive decimal BNM rate for `Nominal` units.
- `RatePerMdlUnit`: normalized `Value / Nominal`.

**Validation**: numeric values are positive decimals and the currency code is
non-empty.

## RateSnapshot

- `RateDate`: BNM publication/effective date.
- `SourceName`: `National Bank of Moldova (BNM)`.
- `Rates`: normalized rates plus MDL = 1.
- `RetrievedAt`: local retrieval timestamp for cache metadata.

**Relationship**: one snapshot contains many exchange rates and is the unit
written to and read from the cache.

## ConversionRequest

- `Amount`: positive decimal entered by the user.
- `SourceCurrency`: selected source code.
- `TargetCurrency`: selected target code.

**Validation**: amount is present, parseable with comma or dot separator,
finite, and greater than zero; both codes are selected.

## ConversionResult

- `Amount`: calculated decimal value.
- `DisplayedAmount`: value rounded to two decimal places.
- `RateDate`: date from the snapshot used.
- `SourceName`: BNM source identity.
- `UsedFallback`: whether the snapshot date differs from requested date.
- `StatusMessage`: freshness, fallback, or availability explanation.

## RateAcquisitionOutcome

- `Snapshot`: usable snapshot when available.
- `UsedFallback`: true when a previous date or local cache was used.
- `RequestedDate`: date initially requested.
- `FailureReason`: user-safe explanation when no snapshot is available.

## State transitions

1. `Idle` → `Loading` when conversion requests rates.
2. `Loading` → `Ready` after a valid BNM snapshot is parsed and cached.
3. `Loading` → `ReadyWithFallback` after a previous-date or cached snapshot is
   selected.
4. `Loading` → `Unavailable` when no usable network or cached snapshot exists.
5. Any state with invalid input → `ValidationError` without a rate request.
6. `Ready` or `ReadyWithFallback` → `ResultDisplayed` after calculation.
