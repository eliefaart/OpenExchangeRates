using System;
using System.Linq;
using System.Collections.Generic;
using OpenExchangeRates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenExchangeRates_Test
{
    /// <summary>
    /// Simple set of tests to test the public methods in the ExchangeRates class
    /// </summary>
    [TestClass]
    public class Test_ExchangeRates
    {
        private const string EUR_STR = "EUR",
                                USD_STR = "USD",
                                CNY_STR = "CNY";
        private const decimal EUR_VAL = 1.0M,
                                USD_VAL = 1.3M,
                                CNY_VAL = 8.0M;

        private ExchangeRates exchangeRates;

        [TestInitialize]
        public void TestInit()
        {
            exchangeRates = new ExchangeRates()
            {
                Disclaimer = "disclaimer",
                License = "license",
                TimeStamp = 0,
                Base = EUR_STR,
                Rates = new Dictionary<string, decimal>()
            };

            exchangeRates.Rates.Add(EUR_STR, EUR_VAL);
            exchangeRates.Rates.Add(USD_STR, USD_VAL);
            exchangeRates.Rates.Add(CNY_STR, CNY_VAL);
        }

        [TestMethod]
        public void Test_GetConversionRate()
        {
            decimal expected, actual;

            //#1
            expected = CNY_VAL / EUR_VAL;
            actual = exchangeRates.GetConversionRate(EUR_STR, CNY_STR);
            Assert.AreEqual(expected, actual);

            //#2
            expected = CNY_VAL / USD_VAL;
            actual = exchangeRates.GetConversionRate(USD_STR, CNY_STR);
            Assert.AreEqual(expected, actual);

            //#3
            expected = EUR_VAL / CNY_VAL;
            actual = exchangeRates.GetConversionRate(CNY_STR, EUR_STR);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Test_GetConversionRate_InvalidCurrency()
        {
            exchangeRates.GetConversionRate(EUR_STR, ":-)");
        }

        [TestMethod]
        public void Test_ConvertInto()
        {
            decimal valueToConvert = 4.50M;
            decimal expected, actual;

            //#1
            expected = valueToConvert * (CNY_VAL / EUR_VAL);
            actual = exchangeRates.ConvertInto(EUR_STR, CNY_STR, valueToConvert);
            Assert.AreEqual(expected, actual);

            //#2
            expected = valueToConvert * (CNY_VAL / USD_VAL);
            actual = exchangeRates.ConvertInto(USD_STR, CNY_STR, valueToConvert);
            Assert.AreEqual(expected, actual);

            //#3
            expected = valueToConvert * (EUR_VAL / CNY_VAL);
            actual = exchangeRates.ConvertInto(CNY_STR, EUR_STR, valueToConvert);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_ConvertInto_List()
        {
            List<decimal> valuesToConvert = new List<decimal>(new decimal[] { 4.50M, 10.20M, 7M});
            List<decimal> expected, actual;

            //#1
            expected = new List<decimal>();
            foreach (var v in valuesToConvert)
                expected.Add(v * (CNY_VAL / EUR_VAL));
            actual = exchangeRates.ConvertInto(EUR_STR, CNY_STR, valuesToConvert);
            Assert.AreEqual(true, Enumerable.SequenceEqual(expected, actual));

            //#2
            expected = new List<decimal>();
            foreach (var v in valuesToConvert)
                expected.Add(v * (USD_VAL / CNY_VAL));
            actual = exchangeRates.ConvertInto(CNY_STR, USD_STR, valuesToConvert);
            Assert.AreEqual(true, Enumerable.SequenceEqual(expected, actual));
        }
    }
}
