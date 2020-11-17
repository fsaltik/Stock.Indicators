﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Internal.Tests
{
    [TestClass]
    public class CmfTests : TestBase
    {

        [TestMethod()]
        public void GetCmfTest()
        {
            int lookbackPeriod = 20;
            List<CmfResult> results = Indicator.GetCmf(history, lookbackPeriod).ToList();

            // assertions

            // proper quantities
            Assert.AreEqual(502, results.Count);
            Assert.AreEqual(483, results.Where(x => x.Cmf != null).Count());

            // sample values
            CmfResult r1 = results[501];
            Assert.AreEqual(0.8052m, Math.Round(r1.MoneyFlowMultiplier, 4));
            Assert.AreEqual(118396116.25m, Math.Round(r1.MoneyFlowVolume, 2));
            Assert.AreEqual(-0.123754m, Math.Round((decimal)r1.Cmf, 6));

            CmfResult r2 = results[249];
            Assert.AreEqual(0.7778m, Math.Round(r2.MoneyFlowMultiplier, 4));
            Assert.AreEqual(36433792.89m, Math.Round(r2.MoneyFlowVolume, 2));
            Assert.AreEqual(-0.040226m, Math.Round((decimal)r2.Cmf, 6));

            CmfResult r3 = results[49];
            Assert.AreEqual(0.5468m, Math.Round(r3.MoneyFlowMultiplier, 4));
            Assert.AreEqual(55609259m, Math.Round(r3.MoneyFlowVolume, 2));
            Assert.AreEqual(0.350596m, Math.Round((decimal)r3.Cmf, 6));
        }


        /* EXCEPTIONS */

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Bad lookback.")]
        public void BadLookbackPeriod()
        {
            Indicator.GetCmf(history, 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(BadHistoryException), "Insufficient history.")]
        public void InsufficientHistory()
        {
            IEnumerable<Quote> h = History.GetHistory(20);
            Indicator.GetCmf(h, 20);
        }
    }
}