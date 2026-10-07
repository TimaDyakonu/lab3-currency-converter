# Feature Specification: BNM Currency Converter

**Feature Branch**: `001-bnm-currency-converter`

**Created**: 2026-10-07

**Status**: Draft

**Input**: User description: "Десктопное приложение с графическим интерфейсом:
конвертер валют по официальным курсам Национального банка Молдовы (BNM)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Convert an Amount (Priority: P1)

As a user, I want to enter an amount, select source and target currencies, and
convert the amount using the latest official BNM rates so that I can see the
equivalent value.

**Why this priority**: This is the core value of the application.

**Independent Test**: Enter a valid positive amount, select two currencies, and
choose "Convert"; the result is displayed with the rate date and data source.

**Acceptance Scenarios**:

1. **Given** a valid positive amount and two selected currencies, **When** the
   user chooses "Convert", **Then** the application displays the converted
   value rounded to two decimal places, the rate date, and the BNM source.
2. **Given** a BNM rate expressed for a nominal quantity other than one,
   **When** the user converts an amount, **Then** the calculation accounts for
   that nominal quantity.
3. **Given** two different foreign currencies, **When** the user converts
   between them, **Then** the result is calculated through their rates against
   the Moldovan leu.
4. **Given** the same currency selected as source and target, **When** the user
   chooses "Convert", **Then** the result equals the entered amount without an
   error.

---

### User Story 2 - Understand Rate Freshness and Fallbacks (Priority: P1)

As a user, I want to know which date and source produced the result and receive
an explicit explanation when current rates are unavailable, so that I can judge
whether the result is suitable for my decision.

**Why this priority**: Rate freshness and transparent fallback behavior are
essential for trustworthy financial information.

**Independent Test**: Simulate unavailable network access, an empty response for
a non-publication day, and a previously saved successful rate; verify that the
application stays usable and clearly identifies the rate date and fallback.

**Acceptance Scenarios**:

1. **Given** no network connection and saved rates, **When** the user requests a
   conversion, **Then** the application does not crash, offers use of the last
   successfully saved rates, and shows their date.
2. **Given** no network connection and no saved rates, **When** the user requests
   a conversion, **Then** the application does not crash and shows a clear
   message that no rate is currently available.
3. **Given** an empty BNM response for a weekend or holiday and a previously
   available rate, **When** the application requests rates, **Then** it uses the
   latest available rate and explicitly states that the displayed rate belongs
   to another date.
4. **Given** a successful rate response, **When** the application is restarted,
   **Then** the saved rates remain available for fallback use.
5. **Given** any displayed conversion result or rate-unavailable state, **Then**
   the interface displays the applicable rate date and identifies BNM as the
   data source, or explains that no rate date is available.

---

### User Story 3 - Correct Invalid Input Before Conversion (Priority: P1)

As a user, I want invalid entries to be identified before calculation so that I
can correct them without the application failing.

**Why this priority**: Preventing invalid financial results is a safety
requirement.

**Independent Test**: Try empty, alphabetic, zero, negative, comma-decimal, and
dot-decimal amounts and verify validation, button state, and conversion result.

**Acceptance Scenarios**:

1. **Given** an empty amount or an unselected currency, **Then** the
   "Convert" button is inactive.
2. **Given** letters, zero, or a negative amount, **When** the user attempts to
   convert, **Then** the application shows a clear validation error and does
   not calculate a result.
3. **Given** a positive amount using either a comma or a dot as decimal
   separator, **When** the user chooses "Convert", **Then** the amount is
   accepted and converted once.

### Edge Cases

- The BNM response is empty, malformed, missing required currency fields, or
  contains a non-positive rate.
- The network request fails or times out while a conversion is being requested.
- A saved cache cannot be read or contains unusable data.
- The requested date is a weekend or public holiday and BNM has not published a
  rate for that date.
- The amount is too large for a meaningful result or has more fractional
  precision than the display supports; the application validates it and
  displays the result to two decimal places.
- The source and target currencies are identical.
- The selected currency is MDL, which must participate in the same conversion
  rules as foreign currencies.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST provide a graphical interface containing an
  amount input, source-currency selector, target-currency selector, a
  "Convert" action, a result area, and rate-date/source information.
- **FR-002**: The application MUST keep the "Convert" action inactive until a
  validly populated amount and both currency selectors are provided.
- **FR-003**: The application MUST obtain official BNM exchange rates in XML
  format from the date-based official source
  `https://www.bnm.md/en/official_exchange_rates?get_xml=1&date=DD.MM.YYYY`.
- **FR-004**: The application MUST interpret each BNM rate together with its
  `Nominal` value and rate currencies against MDL.
- **FR-005**: The application MUST convert in both directions and MUST convert
  between two foreign currencies through MDL.
- **FR-006**: The application MUST round the displayed conversion result to two
  decimal places.
- **FR-007**: Selecting the same source and target currency MUST complete
  successfully and return the entered amount as the result.
- **FR-008**: The application MUST accept both comma and dot decimal
  separators for positive amounts.
- **FR-009**: The application MUST reject empty, alphabetic, zero, and negative
  amounts, show a clear validation message, and skip conversion.
- **FR-010**: If the network is unavailable, the application MUST remain
  running and show a clear message. If a last successful rate is saved, it MUST
  offer that rate as a fallback and show its date.
- **FR-011**: If BNM returns an empty response for a weekend, holiday, or other
  non-publication date, the application MUST use the latest available saved
  rate when one exists and MUST clearly state that another date is being used.
- **FR-012**: If no current or saved usable rate exists, the application MUST
  remain running and state that conversion cannot be completed because no rate
  is available.
- **FR-013**: After every successful source response, the application MUST save
  the latest usable rates locally so they remain available after restart.
- **FR-014**: The application MUST never present a fabricated or silently
  defaulted rate after a network, parsing, cache, or validation failure.
- **FR-015**: The application MUST always display the date of the rate used and
  identify the National Bank of Moldova as the data source when a rate is
  available.
- **FR-016**: Automated tests MUST cover valid conversion, nominal handling,
  MDL intermediary conversion, invalid input, no network, empty response,
  identical currencies, and cache availability after restart. The complete
  test suite MUST be runnable with `dotnet test`.

### Key Entities

- **Currency**: A selectable currency identified by its official code and
  display name, including MDL.
- **Exchange Rate**: A BNM-published rate with currency code, nominal quantity,
  rate against MDL, and publication/effective date.
- **Rate Snapshot**: A complete usable set of exchange rates associated with one
  date and the BNM source.
- **Conversion Request**: A positive amount with source and target currencies.
- **Conversion Result**: The calculated amount, rounded display value, rate date,
  source identity, and any fallback explanation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of acceptance tests, a valid request with usable rates
  produces a result rounded to exactly two displayed decimal places.
- **SC-002**: In 100% of tests for no network, empty source response, malformed
  data, and unreadable cache, the application remains running and shows a
  user-understandable state instead of crashing.
- **SC-003**: In 100% of fallback conversions, the interface identifies both
  the actual rate date and that a fallback date is being used.
- **SC-004**: In 100% of invalid-input tests, no conversion is performed and a
  corrective validation message is shown.
- **SC-005**: A user can complete the primary conversion flow in no more than
  four interactions after entering a valid amount and selecting both
  currencies.
- **SC-006**: After a successful rate retrieval followed by an application
  restart with no network, the last usable snapshot is available in 100% of
  cache persistence tests.

## Assumptions

- The application uses the current local date when requesting the daily BNM
  XML source; the user does not select an arbitrary historical date in the
  initial release.
- The latest usable snapshot is the most recent saved snapshot whose required
  rates parse successfully; no fixed maximum cache age is imposed for this
  release, but its age is always shown to the user.
- BNM's official XML response is the authoritative source and its currency
  codes and date conventions are accepted as published.
- The result area can show explanatory status text in addition to a numeric
  result.
- Mobile, web, account, payment, and trading features are outside the scope of
  this feature.
