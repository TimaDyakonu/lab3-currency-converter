<!--
Sync Impact Report
- Version change: none → 1.0.0 (initial constitution)
- Modified principles: none
- Added sections: Core Principles, Technical Constraints, Development Workflow,
  Governance
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE is unknown and requires confirmation.
-->

# Currency Converter Constitution

## Core Principles

### I. Desktop GUI Only
The product MUST be a desktop application written in C#/.NET with a graphical
user interface. Console interaction MUST NOT be used as the application
interface or as a substitute for required GUI behavior. This keeps the
delivered experience aligned with the product purpose and makes UI behavior
explicitly testable.

### II. Official Exchange-Rate Source
Exchange rates MUST be obtained from the official XML source published by the
National Bank of Moldova. Parsing MUST respect the source schema and MUST
reject empty or unusable responses rather than presenting fabricated rates.
Changes to the source contract MUST be isolated behind the application
logic layer.

### III. Testable Separation of Concerns
Parsing, currency conversion calculations, input validation, and caching MUST
be implemented independently of the graphical interface. These responsibilities
MUST be exposed through testable abstractions or services so unit tests do not
require launching the desktop UI. UI code MUST coordinate these services
instead of duplicating their logic.

### IV. Automated Unit-Test Coverage
Business and data-processing logic MUST be covered by xUnit unit tests.
Tests MUST cover valid conversion, invalid input, malformed or empty source
responses, unavailable network access, and cache behavior. The complete test
suite MUST run with the single command `dotnet test`.

### V. Resilient and Safe Failure Handling
The application MUST remain running and present a clear, user-appropriate
result when the network is unavailable, the source response is empty or
malformed, or user input is invalid. It MUST NOT crash, silently return a
success-shaped fallback, or use an unchecked invalid value in calculations.
When cached rates are available, the application MAY use them according to
the cache policy; otherwise it MUST report that a current rate is unavailable.

## Technical Constraints

The implementation MUST target C#/.NET and use a desktop GUI framework
appropriate for the repository. Network access MUST be abstracted so it can
be replaced by deterministic test doubles. XML parsing, numeric calculations,
validation, and cache storage MUST remain independently replaceable and
testable. User-visible errors MUST be communicated through the GUI without
requiring console output.

Specifications MUST state scope, inputs, outputs, failure behavior, acceptance
criteria, and test expectations sufficiently for another agent to implement
the feature without additional clarification.

## Development Workflow

Every change MUST preserve the constitution's GUI, official-source,
separation-of-concerns, resilience, and testing requirements. New or changed
logic MUST include or update focused xUnit tests, and the full `dotnet test`
command MUST pass before the change is considered complete. Reviews MUST
verify that failure paths are explicit and that no network, parsing, or input
error can terminate the application.

## Governance

This constitution is the highest-level project guidance. If implementation
plans or existing code conflict with it, the conflict MUST be resolved in
favor of this document or explicitly recorded as an amendment before work
continues.

Amendments MUST:

1. Describe the affected principle or governance rule and the reason for the
   change.
2. Update the constitution version and last-amended date.
3. Update directly affected specifications, plans, tasks, and tests.
4. Preserve a clear migration path when existing behavior or requirements
   become incompatible.

Versioning follows semantic rules: MAJOR for backward-incompatible removals or
redefinitions, MINOR for new principles or materially expanded requirements,
and PATCH for clarifications or non-semantic corrections. Compliance MUST be
reviewed during planning, implementation, and code review. Any exception MUST
be documented with its scope, rationale, owner, and expiration or review date.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm original
adoption date | **Last Amended**: 2026-10-07
