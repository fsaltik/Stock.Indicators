﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // DOUBLE EXPONENTIAL MOVING AVERAGE
        public static IEnumerable<EmaResult> GetDoubleEma(IEnumerable<Quote> history, int lookbackPeriod)
        {

            // convert history to basic format
            List<BasicData> bd = Cleaners.ConvertHistoryToBasic(history, "C");

            // validate parameters
            ValidateDema(bd, lookbackPeriod);

            // initialize
            List<EmaResult> results = new List<EmaResult>();
            List<EmaResult> emaN = CalcEma(bd, lookbackPeriod).ToList();

            List<BasicData> bd2 = emaN
                .Where(x => x.Ema != null)
                .Select(x => new BasicData { Date = x.Date, Value = (decimal)x.Ema })
                .ToList();  // note: ToList seems to be required when changing data

            List<EmaResult> emaN2 = CalcEma(bd2, lookbackPeriod).ToList();

            // compose final results
            for (int i = 0; i < emaN.Count; i++)
            {
                EmaResult e1 = emaN[i];
                int index = i + 1;

                EmaResult result = new EmaResult
                {
                    Date = e1.Date
                };

                if (index >= 2 * lookbackPeriod - 1)
                {
                    EmaResult e2 = emaN2[index - lookbackPeriod];
                    result.Ema = 2 * e1.Ema - e2.Ema;
                }

                results.Add(result);
            }

            return results;
        }


        private static void ValidateDema(IEnumerable<BasicData> history, int lookbackPeriod)
        {

            // check parameters
            if (lookbackPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lookbackPeriod), lookbackPeriod,
                    "Lookback period must be greater than 0 for DEMA.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = Math.Max(3 * lookbackPeriod, 2 * lookbackPeriod + 100);
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for DEMA.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.  "
                    + "Since this uses a smoothing technique, for a lookback period of {2}, "
                    + "we recommend you use at least {3} data points prior to the intended "
                    + "usage date for maximum precision.",
                    qtyHistory, minHistory, lookbackPeriod, 2 * lookbackPeriod + 250);

                throw new BadHistoryException(nameof(history), message);
            }

        }

    }
}
