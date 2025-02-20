# Marubozu (Preview)

[Marubozu](https://en.wikipedia.org/wiki/Marubozu) is a candlestick pattern that has no wicks, representing consistent directional movement.
[[Discuss] :speech_balloon:](https://github.com/DaveSkender/Stock.Indicators/discussions/512 "Community discussion about this indicator")

  <img src="pattern.png" alt="drawing" height="150" />

```csharp
// usage
IEnumerable<MarubozuResult> results =
  quotes.GetMarubozu(minBodyPercent);
```

## Parameters

| name | type | notes
| -- |-- |--
| `minBodyPercent` | double | Optional.  Minimum body size as a decimalized percent of total candle size.  Must be between 0.8 and 1, if specified.  Default is 0.95 (95%).

### Historical quotes requirements

You must have at least one historical quote.

`quotes` is an `IEnumerable<TQuote>` collection of historical price quotes.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](../../../docs/GUIDE.md#historical-quotes) for more information.

## Response

```csharp
IEnumerable<MarubozuResult>
```

- This method returns a time series of all available indicator values for the `quotes` provided.
- It always returns the same number of elements as there are in the historical quotes.
- It does not return a single incremental indicator value.
- The candlestick pattern is indicated on dates when `Marubozu` has a non-null value.

### MarubozuResult

| name | type | notes
| -- |-- |--
| `Date` | DateTime | Date
| `Marubozu` | decimal | Indicates a Marubozu candle `Close` price; otherwise `null`
| `IsBullish` | bool | Direction of the candle

### Utilities

- [.Find(lookupDate)](../../../docs/UTILITIES.md#find-indicator-result-by-date)
- [.RemoveWarmupPeriods(qty)](../../../docs/UTILITIES.md#remove-warmup-periods)

See [Utilities and Helpers](../../../docs/UTILITIES.md#utilities-for-indicator-results) for more information.

## Example

```csharp
// fetch historical quotes from your feed (your method)
IEnumerable<Quote> quotes = GetHistoryFromFeed("SPY");

// calculate
IEnumerable<MarubozuResult> results = quotes.GetMarubozu();
```
