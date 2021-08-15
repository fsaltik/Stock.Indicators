﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // PIVOTS (derived from Williams Fractal)
        /// <include file='./info.xml' path='indicator/*' />
        /// 
        public static IEnumerable<PivotsResult> GetPivots<TQuote>(
            this IEnumerable<TQuote> quotes,
            int leftSpan = 2,
            int rightSpan = 2,
            int maxTrendPeriods = 20,
            EndType endType = EndType.HighLow)
            where TQuote : IQuote
        {

            // check parameter arguments
            ValidatePivots(quotes, leftSpan, rightSpan, maxTrendPeriods);

            // initialize

            List<PivotsResult> results
               = GetFractal(quotes, leftSpan, rightSpan, endType)
                .Select(x => new PivotsResult
                {
                    Date = x.Date,
                    HighPoint = x.FractalBear,
                    LowPoint = x.FractalBull
                })
                .ToList();

            int? lastHighIndex = null;
            decimal? lastHighValue = null;
            int? lastLowIndex = null;
            decimal? lastLowValue = null;

            // roll through results
            for (int i = leftSpan; i < results.Count - rightSpan; i++)
            {
                PivotsResult r = results[i];

                // reset expired indexes
                if (lastHighIndex < i - maxTrendPeriods)
                {
                    lastHighIndex = null;
                    lastHighValue = null;
                }

                if (lastLowIndex < i - maxTrendPeriods)
                {
                    lastLowIndex = null;
                    lastLowValue = null;
                }

                // evaluate high trend
                if (r.HighPoint != null)
                {
                    // repaint trend
                    if (lastHighIndex != null && r.HighPoint != lastHighValue)
                    {
                        PivotTrend trend = (r.HighPoint > lastHighValue)
                            ? PivotTrend.HH
                            : PivotTrend.LH;

                        for (int t = (int)lastHighIndex + 1; t <= i; t++)
                        {
                            results[t].HighTrend = trend;
                        }
                    }

                    // reset starting position 
                    lastHighIndex = i;
                    lastHighValue = r.HighPoint;
                }

                // evaluate low trend
                if (r.LowPoint != null)
                {
                    // repaint trend
                    if (lastLowIndex != null && r.LowPoint != lastLowValue)
                    {
                        PivotTrend trend = (r.LowPoint > lastLowValue)
                            ? PivotTrend.HL
                            : PivotTrend.LL;

                        for (int t = (int)lastLowIndex + 1; t <= i; t++)
                        {
                            results[t].LowTrend = trend;
                        }
                    }

                    // reset starting position 
                    lastLowIndex = i;
                    lastLowValue = r.LowPoint;
                }
            }

            return results;
        }

        // parameter validation
        private static void ValidatePivots<TQuote>(
            IEnumerable<TQuote> quotes,
            int leftSpan,
            int rightSpan,
            int maxTrendPeriods)
            where TQuote : IQuote
        {

            // check parameter arguments
            if (rightSpan < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(rightSpan), rightSpan,
                    "Right span must be at least 2 for Pivots.");
            }

            if (leftSpan < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(leftSpan), leftSpan,
                    "Left span must be at least 2 for Pivots.");
            }

            if (maxTrendPeriods <= leftSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(leftSpan), leftSpan,
                    "Lookback periods must be greater than the Left window span for Pivots.");
            }

            // check quotes
            int qtyHistory = quotes.Count();
            int minHistory = leftSpan + rightSpan + 1;
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient quotes provided for Pivots.  " +
                    string.Format(EnglishCulture,
                    "You provided {0} periods of quotes when at least {1} are required.",
                    qtyHistory, minHistory);

                throw new BadQuotesException(nameof(quotes), message);
            }
        }
    }
}
