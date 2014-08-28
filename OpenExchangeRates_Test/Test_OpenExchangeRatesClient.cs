using System;
using System.Collections.Generic;

using OpenExchangeRates;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenExchangeRates_Test
{
    /// <summary>
    /// Set of tests to test the methods in the OpenExchangeRatesClient class.
    /// <remarks>Does not mock the Open Exchange Rates API, so all tests run against the live API right now. This means some of the tests will fail if an invalid
    /// API key is provided or if the API is temporarily not available</remarks>
    /// </summary>
    [TestClass]
    public class Test_OpenExchangeRatesClient
    {
        private const string API_KEY = "<your api key>";

        OpenExchangeRatesClient openExchangeRates;
        PrivateObject privObj;

        [TestInitialize]
        public void TestInit()
        {
            openExchangeRates = new OpenExchangeRatesClient(API_KEY);
            privObj = new PrivateObject(openExchangeRates);
        }





        [TestMethod]
        public void Test_CreateExchangeRatesUrl()
        {
            string expected = "http://openexchangerates.org/api/latest.json?app_id=" + API_KEY;
            string actual = (string)privObj.Invoke("CreateExchangeRatesUrl", new object[] { null, null });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_CreateExchangeRatesUrl_WithBase()
        {
            string baseCur = "EUR";

            string expected = "http://openexchangerates.org/api/latest.json?app_id=" + API_KEY + "&base=" + baseCur;
            string actual = (string)privObj.Invoke("CreateExchangeRatesUrl", new object[] { baseCur, null });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_CreateExchangeRatesUrl_WithSymbols()
        {
            List<string> symbols = new List<string>(new string[] { "EUR", "CNY", "USD" });

            string expected = "http://openexchangerates.org/api/latest.json?app_id=" + API_KEY + "&symbols=EUR,CNY,USD";
            string actual = (string)privObj.Invoke("CreateExchangeRatesUrl", new object[] { null, symbols });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_CreateExchangeRatesUrl_WithBaseAndSymbols()
        {
            string baseCur = "EUR";
            List<string> symbols = new List<string>(new string[] { "EUR", "CNY", "USD" });

            string expected = "http://openexchangerates.org/api/latest.json?app_id=" + API_KEY + "&base=" + baseCur + "&symbols=EUR,CNY,USD";
            string actual = (string)privObj.Invoke("CreateExchangeRatesUrl", new object[] { baseCur, symbols });

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_CreateCurrenciesUrl()
        {
            string expected = "http://openexchangerates.org/api/currencies.json";
            string actual = (string)privObj.Invoke("CreateCurrenciesUrl");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_GetCurrencies()
        {
            List<Currency> currencies = openExchangeRates.GetCurrencies();

            Assert.IsTrue(currencies.Count > 0);
        }

        [TestMethod]
        public void Test_GetExchangeRates()
        {
            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates();

            Assert.IsNotNull(exchangeRates);
            Assert.IsNotNull(exchangeRates.Disclaimer);
            Assert.IsNotNull(exchangeRates.License);
            Assert.IsNotNull(exchangeRates.TimeStamp);
            Assert.IsNotNull(exchangeRates.Base);
            Assert.IsTrue(exchangeRates.Rates.Count > 0);
        }

        [TestMethod]
        public void Test_FullLiveTest()
        {
            List<Currency> currencies = openExchangeRates.GetCurrencies();
            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates();

            Assert.IsNotNull(currencies);
            Assert.IsNotNull(exchangeRates);
            Assert.IsNotNull(exchangeRates.Disclaimer);
            Assert.IsNotNull(exchangeRates.License);
            Assert.IsNotNull(exchangeRates.TimeStamp);
            Assert.IsNotNull(exchangeRates.Base);
            Assert.IsNotNull(exchangeRates.Rates);

            Assert.IsTrue(currencies.Count > 0 && currencies.Count == exchangeRates.Rates.Count);

            foreach (var c in currencies)
            {
                decimal d;
                if (!exchangeRates.Rates.TryGetValue(c.Code, out d))
                {
                    Assert.Fail("Currency " + c.Code + " does not exist in the retrieved exchange rates");
                }
            }
        }
    }
}
