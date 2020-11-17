﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // ZIG ZAG
        public static IEnumerable<ZigZagResult> GetZigZag(
            IEnumerable<Quote> history, ZigZagType type = ZigZagType.Close, decimal percentChange = 5)
        {

            // clean quotes
            List<Quote> historyList = history.Sort();

            // check parameters
            ValidateZigZag(history, percentChange);

            // initialize
            List<ZigZagResult> results = new List<ZigZagResult>();
            decimal changeThreshold = percentChange / 100m;
            Quote firstQuote = historyList[0];
            ZigZagEval eval = GetZigZagEval(type, 1, firstQuote);

            ZigZagPoint lastPoint = new ZigZagPoint
            {
                Index = eval.Index,
                Value = firstQuote.Close,
                PointType = "U"
            };

            ZigZagPoint lastHighPoint = new ZigZagPoint
            {
                Index = eval.Index,
                Value = eval.High,
                PointType = "H"
            };

            ZigZagPoint lastLowPoint = new ZigZagPoint
            {
                Index = eval.Index,
                Value = eval.Low,
                PointType = "L"
            };

            int finalPointIndex = historyList.Count;

            // roll through history until to find initial trend
            for (int i = 0; i < historyList.Count; i++)
            {
                Quote h = historyList[i];
                int index = i + 1;

                eval = GetZigZagEval(type, index, h);
                decimal changeUp = (eval.High - lastLowPoint.Value) / lastLowPoint.Value;
                decimal changeDn = (lastHighPoint.Value - eval.Low) / lastHighPoint.Value;

                if (changeUp >= changeThreshold && changeUp > changeDn)
                {
                    lastPoint.Index = lastLowPoint.Index;
                    lastPoint.Value = lastLowPoint.Value;
                    lastPoint.PointType = lastLowPoint.PointType;
                    break;
                }

                if (changeDn >= changeThreshold && changeDn > changeUp)
                {
                    lastPoint.Index = lastHighPoint.Index;
                    lastPoint.Value = lastHighPoint.Value;
                    lastPoint.PointType = lastHighPoint.PointType;
                    break;
                }
            }

            // add first point to results
            ZigZagResult firstResult = new ZigZagResult
            {
                Date = firstQuote.Date
            };
            results.Add(firstResult);

            // find and draw lines
            while (lastPoint.Index < finalPointIndex)
            {
                ZigZagPoint nextPoint = EvaluateNextPoint(historyList, type, changeThreshold, lastPoint);
                string lastDirection = lastPoint.PointType;

                // draw line (and reset last point)
                DrawZigZagLine(results, historyList, lastPoint, nextPoint);

                // draw retrace line (and reset last high/low point)
                DrawRetraceLine(results, lastDirection, lastLowPoint, lastHighPoint, nextPoint);
            }

            return results;
        }


        private static ZigZagPoint EvaluateNextPoint(List<Quote> historyList,
            ZigZagType type, decimal changeThreshold, ZigZagPoint lastPoint)
        {
            // initialize 
            bool trendUp = (lastPoint.PointType == "L");
            decimal change = 0;

            ZigZagPoint extremePoint = new ZigZagPoint
            {
                Index = lastPoint.Index,
                Value = lastPoint.Value,
                PointType = trendUp ? "H" : "L"
            };

            // find extreme point before reversal point
            for (int i = lastPoint.Index; i < historyList.Count; i++)
            {
                Quote h = historyList[i];
                int index = i + 1;

                ZigZagEval eval = GetZigZagEval(type, index, h);

                // reset extreme point
                switch (trendUp)
                {
                    case true:

                        if (eval.High >= extremePoint.Value)
                        {
                            extremePoint.Index = eval.Index;
                            extremePoint.Value = eval.High;
                        }
                        else
                        {
                            change = (extremePoint.Value - eval.Low) / extremePoint.Value;
                        }

                        break;

                    case false:

                        if (eval.Low <= extremePoint.Value)
                        {
                            extremePoint.Index = eval.Index;
                            extremePoint.Value = eval.Low;
                        }
                        else
                        {
                            change = (eval.High - extremePoint.Value) / extremePoint.Value;
                        }

                        break;
                }

                // return extreme point when deviation threshold met
                if (change >= changeThreshold)
                {
                    return extremePoint;
                }
            }

            // handle last unconfirmed point
            int finalPointIndex = historyList.Count;
            if (extremePoint.Index == finalPointIndex && change < changeThreshold)
            {
                extremePoint.PointType = null;
            }

            return extremePoint;
        }


        private static void DrawZigZagLine(List<ZigZagResult> results, List<Quote> historyList,
            ZigZagPoint lastPoint, ZigZagPoint nextPoint)
        {

            decimal increment = (nextPoint.Value - lastPoint.Value) / (nextPoint.Index - lastPoint.Index);

            // add new line segment
            for (int i = lastPoint.Index; i < nextPoint.Index; i++)
            {
                Quote h = historyList[i];
                int index = i + 1;

                ZigZagResult result = new ZigZagResult
                {
                    Date = h.Date,
                    ZigZag = (lastPoint.Index != 1 || index == nextPoint.Index) ?
                        lastPoint.Value + increment * (index - lastPoint.Index) : null,
                    PointType = (index == nextPoint.Index) ? nextPoint.PointType : null
                };

                results.Add(result);
            }

            // reset lastpoint
            lastPoint.Index = nextPoint.Index;
            lastPoint.Value = nextPoint.Value;
            lastPoint.PointType = nextPoint.PointType;
        }


        private static void DrawRetraceLine(List<ZigZagResult> results, string lastDirection,
            ZigZagPoint lastLowPoint, ZigZagPoint lastHighPoint, ZigZagPoint nextPoint)
        {
            bool isHighLine = (lastDirection == "L");
            ZigZagPoint priorPoint = new ZigZagPoint();

            // handle type and reset last point
            if (isHighLine)
            {
                priorPoint.Index = lastHighPoint.Index;
                priorPoint.Value = lastHighPoint.Value;

                lastHighPoint.Index = nextPoint.Index;
                lastHighPoint.Value = nextPoint.Value;
            }
            else
            {
                priorPoint.Index = lastLowPoint.Index;
                priorPoint.Value = lastLowPoint.Value;

                lastLowPoint.Index = nextPoint.Index;
                lastLowPoint.Value = nextPoint.Value;
            }

            // nothing to do if first line
            if (priorPoint.Index == 1)
            {
                return;
            }

            // narrow to period
            decimal increment = (nextPoint.Value - priorPoint.Value) / (nextPoint.Index - priorPoint.Index);

            // add new line segment
            //foreach (ZigZagResult r in period)
            for (int i = priorPoint.Index - 1; i < nextPoint.Index; i++)
            {
                ZigZagResult r = results[i];
                int index = i + 1;

                if (isHighLine)
                {
                    r.RetraceHigh = priorPoint.Value + increment * (index - priorPoint.Index);
                }
                else
                {
                    r.RetraceLow = priorPoint.Value + increment * (index - priorPoint.Index);
                }
            }
        }


        private static ZigZagEval GetZigZagEval(ZigZagType type, int index, Quote q)
        {
            ZigZagEval eval = new ZigZagEval()
            {
                Index = index
            };

            // consider `type`
            switch (type)
            {
                case ZigZagType.Close:

                    eval.Low = q.Close;
                    eval.High = q.Close;
                    break;

                case ZigZagType.HighLow:

                    eval.Low = q.Low;
                    eval.High = q.High;
                    break;
            }

            return eval;
        }


        private static void ValidateZigZag(IEnumerable<Quote> history, decimal percentChange)
        {

            // check parameters
            if (percentChange <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(percentChange), percentChange,
                    "Percent change must be greater than 0 for ZIGZAG.");
            }

            // check histor
            int qtyHistory = history.Count();
            int minHistory = 2;
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for ZIGZAG.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.",
                    qtyHistory, minHistory);

                throw new BadHistoryException(nameof(history), message);
            }

        }
    }

}
