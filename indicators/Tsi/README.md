﻿# True Strength Index (TSI)

Created by William Blau, the [True Strength Index](https://en.wikipedia.org/wiki/True_strength_index) is a momentum oscillator that depicts trends in price changes.
[[Discuss] :speech_balloon:](https://github.com/DaveSkender/Stock.Indicators/discussions/300 "Community discussion about this indicator")

![image](chart.png)

```csharp
// usage
IEnumerable<TsiResult> results = 
  quotes.GetTsi(lookbackPeriods, smoothPeriods, signalPeriods);  
```

## Parameters

| name | type | notes
| -- |-- |--
| `lookbackPeriods` | int | Number of periods (`N`) for the first EMA.  Must be greater than 0.  Default is 25.
| `smoothPeriods` | int | Number of periods (`M`) for the second smoothing.  Must be greater than 0.  Default is 13.
| `signalPeriods` | int | Number of periods (`S`) in the TSI moving average.  Must be greater than or equal to 0.  Default is 7.

### Historical quotes requirements

You must have at least `N+M+100` periods of `quotes`.  Since this uses a two EMA smoothing techniques, we recommend you use at least `N+M+250` data points prior to the intended usage date for better precision.

`quotes` is an `IEnumerable<TQuote>` collection of historical price quotes.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](../../docs/GUIDE.md#historical-quotes) for more information.

## Response

```csharp
IEnumerable<TsiResult>
```

- This method returns a time series of all available indicator values for the `quotes` provided.
- It always returns the same number of elements as there are in the historical quotes.
- It does not return a single incremental indicator value.
- The first `N+M-1` periods will have `null` values since there's not enough data to calculate.
- `Signal` will be `null` for all periods if `signalPeriods=0`.

:warning: **Warning**: The first `N+M+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.

### TsiResult

| name | type | notes
| -- |-- |--
| `Date` | DateTime | Date
| `Tsi` | decimal | True Strength Index
| `Signal` | decimal | Signal line (EMA of TSI)

### Utilities

- [.Find(lookupDate)](../../docs/UTILITIES.md#find-indicator-result-by-date)
- [.RemoveWarmupPeriods()](../../docs/UTILITIES.md#remove-warmup-periods)
- [.RemoveWarmupPeriods(qty)](../../docs/UTILITIES.md#remove-warmup-periods)

See [Utilities and Helpers](../../docs/UTILITIES.md#utilities-for-indicator-results) for more information.

## Example

```csharp
// fetch historical quotes from your feed (your method)
IEnumerable<Quote> quotes = GetHistoryFromFeed("MSFT");

// calculate 20-period TSI
IEnumerable<TsiResult> results = quotes.GetTsi(25,13,7);
```
