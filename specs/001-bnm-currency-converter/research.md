# Research: BNM Currency Converter

## Decision: Use the official BNM date-based XML endpoint

**Decision**: Request `https://www.bnm.md/en/official_exchange_rates?get_xml=1&date=DD.MM.YYYY`
for the current date and, when the response has no usable rates, retry the
previous date up to seven preceding calendar days.

**Rationale**: BNM may not publish rates on weekends or holidays. A bounded
backward search provides a deterministic fallback without unbounded requests.

**Alternatives considered**: JSON is not available for this source. Searching
indefinitely or relying only on the local cache would either delay failure or
miss a newly published rate.

## Decision: Parse rates into normalized MDL base values

**Decision**: Parse the XML date, currency code, `Nominal`, and `Value`. Store
the normalized value per one unit of foreign currency as `Value / Nominal`,
while representing MDL as exactly 1 MDL per MDL.

**Rationale**: BNM quotes foreign currencies against MDL and may quote a rate
for 10, 100, or another nominal quantity. Normalization makes conversion use
`sourceAmount * sourceMdlRate / targetMdlRate`.

**Alternatives considered**: Keeping nominal and applying it in every
calculation risks inconsistent handling. A hard-coded intermediary currency
would violate the MDL base contract.

## Decision: Isolate retrieval behind `IRatesProvider`

**Decision**: Core exposes an asynchronous provider contract returning either a
usable rate snapshot or an explicit unavailable/fallback status. The production
provider uses `HttpClient` with a finite timeout; tests inject a deterministic
stub.

**Rationale**: Network behavior, timeouts, empty responses, and retry dates must
be testable without live BNM access or a UI process.

**Alternatives considered**: Calling `HttpClient` directly from the ViewModel
would couple UI behavior to transport and make failure-path tests fragile.

## Decision: Persist the latest successful snapshot as JSON

**Decision**: Save one validated snapshot, including rate date, source identity,
normalized rates, and original nominal metadata, below the application's
LocalApplicationData directory. Cache read/write errors are non-fatal status
outcomes.

**Rationale**: A single portable file meets restart persistence and avoids a
database dependency.

**Alternatives considered**: An in-memory cache does not survive restart;
registry storage is less portable; a database is unnecessary for one snapshot.

## Decision: Keep UI asynchronous and thin

**Decision**: The WPF ViewModel invokes Core asynchronously, exposes bindable
input/status/result properties, and never parses XML or calculates rates.

**Rationale**: This preserves responsiveness and separation of concerns while
making the primary flow directly testable at the Core boundary.

**Alternatives considered**: Code-behind orchestration would duplicate
business behavior and make UI error paths harder to test.

## Verified source details and defensive rules

The BNM response is a `ValCurs` document with a `Date` attribute and `Valute`
children. Each `Valute` contains `CharCode`, `Nominal`, `Name`, and `Value`;
the `Valute` `ID` is not the currency key. `Value` is the MDL amount for the
specified `Nominal`, so the parser must use an invariant culture and reject
missing or non-positive fields, empty rate sets, malformed XML, and a missing
effective date.

The returned `ValCurs/@Date` is authoritative. The provider must display that
effective date rather than assuming it equals the requested date. HTTP 404,
empty bodies, and non-publication responses are unavailable candidates eligible
for the bounded previous-day search; malformed or incomplete XML is never
converted into an empty successful snapshot.

`HttpClient` is application-scoped and injected, with a finite timeout and
operation cancellation. Cache writes happen only after complete validation and
use a temporary file followed by replacement where supported. Cache data is
validated on read and visibly labeled as cached/stale in the UI.
