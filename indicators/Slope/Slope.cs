﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // SLOPE AND LINEAR REGRESSION
        public static IEnumerable<SlopeResult> GetSlope(IEnumerable<Quote> history, int lookbackPeriod)
        {
            // clean quotes
            List<Quote> historyList = history.Sort();

            // validate parameters
            ValidateSlope(history, lookbackPeriod);

            // initialize
            List<SlopeResult> results = new List<SlopeResult>();

            // roll through history for interim data
            for (int i = 0; i < historyList.Count; i++)
            {
                Quote h = historyList[i];
                int index = i + 1;

                SlopeResult r = new SlopeResult
                {
                    Date = h.Date
                };

                results.Add(r);

                // skip initialization period
                if (index < lookbackPeriod)
                {
                    continue;
                }

                // get averages for period
                decimal sumX = 0m;
                decimal sumY = 0m;

                for (int p = index - lookbackPeriod; p < index; p++)
                {
                    Quote d = historyList[p];

                    sumX += p + 1m;
                    sumY += d.Close;
                }

                decimal avgX = sumX / lookbackPeriod;
                decimal avgY = sumY / lookbackPeriod;

                // least squares method
                decimal sumSqX = 0m;
                decimal sumSqY = 0m;
                decimal sumSqXY = 0m;

                for (int p = index - lookbackPeriod; p < index; p++)
                {
                    Quote d = historyList[p];

                    decimal devX = (p + 1m - avgX);
                    decimal devY = (d.Close - avgY);

                    sumSqX += devX * devX;
                    sumSqY += devY * devY;
                    sumSqXY += devX * devY;
                }

                r.Slope = sumSqXY / sumSqX;
                r.Intercept = avgY - r.Slope * avgX;

                // calculate Standard Deviation and R-Squared
                double stdDevX = Math.Sqrt((double)sumSqX / lookbackPeriod);
                double stdDevY = Math.Sqrt((double)sumSqY / lookbackPeriod);
                r.StdDev = stdDevY;

                if (stdDevX * stdDevY != 0)
                {
                    double R = ((double)sumSqXY / (stdDevX * stdDevY)) / lookbackPeriod;
                    r.RSquared = R * R;
                }
            }

            // add last Line (y = mx + b)
            int lastIndex = historyList.Count;
            SlopeResult last = results[lastIndex - 1];

            for (int p = lastIndex - lookbackPeriod; p < lastIndex; p++)
            {
                SlopeResult d = results[p];
                d.Line = last.Slope * (p + 1) + last.Intercept;
            }

            return results;
        }


        private static void ValidateSlope(IEnumerable<Quote> history, int lookbackPeriod)
        {

            // check parameters
            if (lookbackPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lookbackPeriod), lookbackPeriod,
                    "Lookback period must be greater than 0 for Slope/Linear Regression.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = lookbackPeriod;
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for Slope/Linear Regression.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.",
                    qtyHistory, minHistory);

                throw new BadHistoryException(nameof(history), message);
            }
        }
    }

}
