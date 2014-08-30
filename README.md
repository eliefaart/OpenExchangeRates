OpenExchangeRates
=================

C#.Net class library for querying the openexchangerates.org REST API.

Features 
========
* Retrieve the full list of currencies supported by Open Exchange Rates
* Retrieve the current exchange rates for currencies
    Specify base currency for exchange rates
    Limit list of returned exchange rates to specific currencies

Supports HTTP ETag
==================
http://en.wikipedia.org/wiki/HTTP_ETag
Responses from the server are cached and whenever the server is queried again for the latest exchange rates, the etag from the previous response is included in the new request. This means the server will only give a full response back if the data since the previous request was updated, otherwise an empty response with HTTP code 304 (Not Modified) is returned. This potentially reduces some bandwidth.

Basic usage
===========
OpenExchangeRatesClient oerClient = new OpenExchangeRatesClient("<your api key>");

// Get all supported currencies
List<Currency> currencies = oerClient.GetCurrencies();

try
{
  // Get all supported currencies
  ExchangeRates exchangeRates = oerClient.GetExchangeRates();

  // All rates and other data is stored in the ExchangeRates object and can be accessed using the public properties
  string baseCurrency = exchangeRates.Base;
  decimal rateEUR = exchangeRates.Rates["EUR"];

  // The ExchangeRates object has methods to convert currencies that uses the rates stored within the instance of the object
  decimal convertedValue = exchangeRates.ConvertCurrency("EUR", "USD", 13.45M);
}
catch (APIErrorException aee)
{
  // If the server returns an error, this error is stored in the ErrorMessage object
  ErrorMessage errorMessage = aee.ErrorMessage;
  Console.Error.WriteLine(errorMessage.ToString());
}
catch (Exception) { }
