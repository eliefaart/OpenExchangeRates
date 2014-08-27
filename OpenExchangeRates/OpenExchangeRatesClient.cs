using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace OpenExchangeRates
{
    public class OpenExchangeRatesClient
    {
        private const string API_BASE_URL = "http://openexchangerates.org/api/";
        private const string API_EXCHANGE_RATES_PARAM = "latest.json";
        private const string API_CURRENCIES_PARAM = "currencies.json";
        private const string API_APP_ID_PREFIX = "?app_id=";
        private const string API_BASE_CURRENCY_PREFIX = "&base=";
        private const string API_SYMBOLS_PREFIX = "&symbols=";

        private string APIKey;
        private bool useHTTPS;

        /// <summary>
        /// Class containing the cached data/response from a previous request to the API.
        /// Open Exchange Rates supports ETag (Entity Tag). This means that when made use of, the server will only give a full response if the data
        /// has actually been updated since the last request. This means the responses from the server should be cached, so the cached json can be used
        /// if the server indicated this data is still up-to-date. This will save some bandwidth.
        /// </summary>
        private class OERCache
        {
            public string cached_responseHeaderETag;
            public string cached_responseHeaderData;
            public string cached_responseJSon;
        }

        // Holds the cached data for each OER-url. Each unique url holds its own cached data, 
        // so for example each url with a different base currency specified has its own cached data
        private Dictionary<string, OERCache> oerUrlCaches;

        //  Constructors
        public OpenExchangeRatesClient(string APIKey) : this(APIKey, false) { }
        public OpenExchangeRatesClient(string APIKey, bool useHTTPS)
        {
            if (APIKey == null || APIKey == String.Empty)
                throw new Exception("API Key empty.");

            this.APIKey = APIKey;
            this.useHTTPS = useHTTPS;

            oerUrlCaches = new Dictionary<string, OERCache>();
        }

        /// <summary>
        /// Retrieve the current exchange rates 
        /// </summary>
        /// <param name="baseCurrency">Base currency</param>
        /// <param name="currenciesToRetrieve">List of which currencies (Currency codes) to get the exchange rates for</param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="APIErrorException">Thrown when the API returns an error</exception>
        /// <returns>Exchange rates</returns>
        public ExchangeRates GetExchangeRates(string baseCurrency = null, List<string> currenciesToRetrieve = null)
        {
            string url = CreateExchangeRatesUrl(baseCurrency, currenciesToRetrieve);
            string json = GetJSonResponseFromOERUrl(url);

            return JsonConvert.DeserializeObject<ExchangeRates>(json);
        }

        /// <summary>
        /// Retrieve the full list of currencies supported by Open Exchange Rates
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="APIErrorException">Thrown when the API returns an error</exception>
        /// <returns>List of currencies</returns>
        public List<Currency> GetCurrencies()
        {
            string url = CreateCurrenciesUrl();
            string json = GetJSonResponseFromOERUrl(url);

            Dictionary<string, string> currencies = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            List<Currency> curList = new List<Currency>();
            foreach (var cur in currencies)
            {
                curList.Add(new Currency()
                {
                    Code = cur.Key,
                    FullName = cur.Value
                });
            }

            return curList;
        }






        /// <summary>
        /// Send a API request to the given url and retrieve the JSon string that the response from the open exchange rates API will contain.
        /// </summary>
        /// <param name="url">API url</param>
        /// <returns>JSon</returns>
        private string GetJSonResponseFromOERUrl(string url)
        {
            OERCache cache;

            // Get the cached data for this url, if it exists
            if (!oerUrlCaches.TryGetValue(url, out cache))
                cache = null;

            // Build the HTTP request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentLength = 0;

            if (cache != null)
            {
                request.Headers.Add(HttpRequestHeader.IfNoneMatch, cache.cached_responseHeaderETag);
                request.IfModifiedSince = DateTime.Parse(cache.cached_responseHeaderData);
            }

            HttpWebResponse response;
            try
            {
                // For some HTTP status codes being returned, an exception is thrown. 
                // Catch this exception, grab the WebResponse from the exception object and deal with the response outside the try-catch block.
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:             //200
                    {
                        string json = GetResponseFromHttpWebResponse(response);
                        string etag = response.Headers[HttpResponseHeader.ETag.ToString()];
                        string date = response.Headers[HttpResponseHeader.Date.ToString()];

                        if (etag != null && date != null)
                        {
                            // Server included the ETag and date headers, so we can cache the response
                            cache = new OERCache()
                            {
                                cached_responseHeaderETag = etag,
                                cached_responseHeaderData = date,
                                cached_responseJSon = json,
                            };

                            // Add/update the cached data for this url
                            if (oerUrlCaches.ContainsKey(url))
                                oerUrlCaches[url] = cache;
                            else
                                oerUrlCaches.Add(url, cache);
                        }

                        return json;
                    }
                case HttpStatusCode.NotModified:    //304
                    {
                        // Nothing changed since last request, leave the cache intact and return the cached json
                        return cache.cached_responseJSon;
                    }
                case HttpStatusCode.BadRequest:     //400
                case HttpStatusCode.Unauthorized:   //401
                case HttpStatusCode.NotFound:       //404
                case HttpStatusCode.Forbidden:      //403
                case (HttpStatusCode)429:           //429 'Too many requests' is not included in the HttpStatusCode enum
                    {
                        string json = GetResponseFromHttpWebResponse(response);
                        ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(json);

                        if (oerUrlCaches.ContainsKey(url))
                            oerUrlCaches.Remove(url);

                        throw new APIErrorException(errorMessage);
                    }
                default:
                    {
                        if (oerUrlCaches.ContainsKey(url))
                            oerUrlCaches.Remove(url);
                        throw new Exception("Unexpected HTTP Status code: " + response.StatusCode);
                    }
            }
        }

        private string CreateExchangeRatesUrl(string baseCurrency = null, List<string> currenciesToRetrieve = null)
        {
            string url = String.Empty;
            url += useHTTPS ? API_BASE_URL.Replace("http", "https") : API_BASE_URL;
            url += API_EXCHANGE_RATES_PARAM;
            url += API_APP_ID_PREFIX + APIKey;

            // Indicate what the base currency of the response should be, if desired
            if (baseCurrency != null && baseCurrency != String.Empty)
                url += API_BASE_CURRENCY_PREFIX + baseCurrency;

            // Indicate the response should only include certain currencies, if desired
            if (currenciesToRetrieve != null && currenciesToRetrieve.Count > 0)
            {
                url += API_SYMBOLS_PREFIX;

                foreach (var cur in currenciesToRetrieve)
                    url += cur + ",";

                // Remove the comma at the very end of the url
                url = url.TrimEnd(',');
            }

            return url;
        }

        private string CreateCurrenciesUrl()
        {
            string url = String.Empty;
            url += useHTTPS ? API_BASE_URL.Replace("http", "https") : API_BASE_URL; ;
            url += API_CURRENCIES_PARAM;

            return url;
        }

        private string GetResponseFromHttpWebResponse(HttpWebResponse response)
        {
            Stream stream = response.GetResponseStream();
            byte[] buffer;
            using (MemoryStream memStream = new MemoryStream((int)response.ContentLength))
            {
                byte[] part = new byte[4096];
                int bytesRead;
                while ((bytesRead = stream.Read(part, 0, part.Length)) > 0)
                {
                    memStream.Write(part, 0, bytesRead);
                }
                buffer = memStream.ToArray();
            }

            return System.Text.Encoding.UTF8.GetString(buffer);
        }
    }
}