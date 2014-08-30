using System;
using System.Collections.Generic;

using OpenExchangeRates;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenExchangeRates_Test
{
    /// <summary>
    /// Set of tests to test the methods in the OpenExchangeRatesClient class.
    /// <remarks>
    /// Does not mock the Open Exchange Rates API, so some tests run against the live API right now. This means some of the tests will fail if an invalid
    /// API key is provided or if the API is temporarily not available. If you want to skip these tests, set SKIP_ALL_LIVE_TESTS to true.
    /// </remarks>
    /// </summary>
    [TestClass]
    public class Test_OpenExchangeRatesClient
    {
        private const bool SKIP_ALL_LIVE_TESTS = true;      // This will skip all tests that use the live API if set to true
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
            if (SKIP_ALL_LIVE_TESTS)
                return;

            List<Currency> currencies = openExchangeRates.GetCurrencies();

            Assert.IsTrue(currencies.Count > 0);
        }

        [TestMethod]
        public void Test_GetExchangeRates()
        {
            if (SKIP_ALL_LIVE_TESTS)
                return;

            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates();

            Assert.IsNotNull(exchangeRates);
            Assert.IsNotNull(exchangeRates.Disclaimer);
            Assert.IsNotNull(exchangeRates.License);
            Assert.IsNotNull(exchangeRates.TimeStamp);
            Assert.IsNotNull(exchangeRates.Base);
            Assert.IsTrue(exchangeRates.Rates.Count > 0);
        }

        [TestMethod]
        public void Test_GetExchangeRates_WithBase()
        {
            if (SKIP_ALL_LIVE_TESTS)
                return;

            string desiredBase = "CNY";
            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates(desiredBase);

            Assert.AreEqual(desiredBase, exchangeRates.Base);
        }

        [TestMethod]
        public void Test_GetExchangeRates_WithBaseAndSymbols()
        {
            if (SKIP_ALL_LIVE_TESTS)
                return;

            string desiredBase = "CNY";
            List<string> symbols = new List<string>(new string[] {"EUR, CNY, USD"});
            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates(desiredBase, symbols);

            Assert.AreEqual(desiredBase, exchangeRates.Base);
            Assert.AreEqual(symbols.Count, exchangeRates.Rates.Count);

            foreach (var c in exchangeRates.Rates.Keys)
                Assert.IsTrue(symbols.Contains(c));
        }

        [TestMethod]
        [ExpectedException(typeof(APIErrorException))]
        public void Test_GetExchangeRates_WithBaseButNotAllowed()
        {
            if (SKIP_ALL_LIVE_TESTS)
                throw new APIErrorException(null);  // Test expects an exception, so throw it to skip and mark test passed

            openExchangeRates = new OpenExchangeRatesClient("*doesntmatter*");
            ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates("EUR", null);
        }

        [TestMethod]
        public void Test_GetExchangeRates_WithBaseButNotAllowed_CatchException()
        {
            if (SKIP_ALL_LIVE_TESTS)
                return;

            ErrorMessage errorMessage = null;
            openExchangeRates = new OpenExchangeRatesClient("*doesntmatter*");
            try
            {
                ExchangeRates exchangeRates = openExchangeRates.GetExchangeRates("EUR", null);
            }
            catch (APIErrorException aee)
            {
                errorMessage = aee.ErrorMessage;
            }

            Assert.IsNotNull(errorMessage);
            Assert.IsNotNull(errorMessage.Error);
            Assert.IsNotNull(errorMessage.Status);
            Assert.IsNotNull(errorMessage.Message);
            Assert.IsNotNull(errorMessage.Description);
        }

        [TestMethod]
        public void Test_FullLiveTest()
        {
            if (SKIP_ALL_LIVE_TESTS)
                return;

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
