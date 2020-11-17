﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // DONCHIAN CHANNEL
        public static IEnumerable<KeltnerResult> GetKeltner(
            IEnumerable<Quote> history, int emaPeriod = 20, decimal multiplier = 2, int atrPeriod = 10)
        {

            // clean quotes
            List<Quote> historyList = history.Sort();

            // validate parameters
            ValidateKeltner(history, emaPeriod, multiplier, atrPeriod);

            // initialize
            List<KeltnerResult> results = new List<KeltnerResult>();
            List<EmaResult> emaResults = GetEma(history, emaPeriod).ToList();
            List<AtrResult> atrResults = GetAtr(history, atrPeriod).ToList();
            int lookbackPeriod = Math.Max(emaPeriod, atrPeriod);

            // roll through history
            for (int i = 0; i < historyList.Count; i++)
            {
                Quote h = historyList[i];
                int index = i + 1;

                KeltnerResult result = new KeltnerResult
                {
                    Date = h.Date
                };

                if (index >= lookbackPeriod)
                {
                    EmaResult ema = emaResults[i];
                    AtrResult atr = atrResults[i];

                    result.UpperBand = ema.Ema + multiplier * atr.Atr;
                    result.LowerBand = ema.Ema - multiplier * atr.Atr;
                    result.Centerline = ema.Ema;
                    result.Width = (result.Centerline == 0) ? null
                        : (result.UpperBand - result.LowerBand) / result.Centerline;
                }

                results.Add(result);
            }

            return results;
        }


        private static void ValidateKeltner(
            IEnumerable<Quote> history, int emaPeriod, decimal multiplier, int atrPeriod)
        {

            // check parameters
            if (emaPeriod <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(emaPeriod), emaPeriod,
                    "EMA period must be greater than 1 for Keltner Channel.");
            }

            if (atrPeriod <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(atrPeriod), atrPeriod,
                    "ATR period must be greater than 1 for Keltner Channel.");
            }

            if (multiplier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                    "Multiplier must be greater than 0 for Keltner Channel.");
            }

            // check history
            int lookbackPeriod = Math.Max(emaPeriod, atrPeriod);
            int qtyHistory = history.Count();
            int minHistory = Math.Max(2 * lookbackPeriod, lookbackPeriod + 100);
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for Keltner Channel.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.  "
                    + "Since this uses a smoothing technique, for a lookback period of {2}, "
                    + "we recommend you use at least {3} data points prior to the intended "
                    + "usage date for maximum precision.",
                    qtyHistory, minHistory, lookbackPeriod, lookbackPeriod + 250);

                throw new BadHistoryException(nameof(history), message);
            }
        }

    }

}
