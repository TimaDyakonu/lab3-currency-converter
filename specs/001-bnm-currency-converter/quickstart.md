# Quickstart Validation Guide

## Prerequisites

- Windows with the .NET 8 SDK installed.
- Network access is needed only for live BNM retrieval or manual use.

## Build and test

From the repository root:

```powershell
dotnet build
dotnet test
```

Both commands must complete successfully without manual project setup.

## Manual acceptance flow

1. Launch the WPF application from the built solution.
2. Confirm the amount field, source/target selectors, Convert button, result,
   rate date, and BNM source are visible.
3. Enter `12,50` and `12.50` in separate attempts; select currencies and
   convert. Confirm a two-decimal result.
4. Select the same source and target; confirm the result equals the input.
5. Disable network after one successful retrieval. Confirm the application
   remains open, identifies the cached date, and offers cached rates.
6. Restart with network unavailable; confirm the saved JSON snapshot remains
   usable.
7. Enter empty text, letters, zero, and a negative value. Confirm the Convert
   action is disabled where required or a validation message prevents
   calculation.

## Automated scenarios

The xUnit suite must cover XML nominal and malformed/empty behavior, provider
timeout/network failure, empty-date response, seven-day search limit, cache
fallback, direct/reverse/MDL-mediated/same-currency conversion, comma/dot
parsing, invalid inputs, and persistence across separately created cache
service instances.

See [data-model.md](./data-model.md) and
[contracts/core-services.md](./contracts/core-services.md) for boundaries and
state expectations.
