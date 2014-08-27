using System;

namespace OpenExchangeRates
{
    /// <summary>
    /// An exception object that also holds the error message returned from the Open Exchange Rates API.
    /// </summary>
    public class APIErrorException : Exception
    {
        public ErrorMessage ErrorMessage { get; private set; }

        public APIErrorException(ErrorMessage errorMessage) : this(String.Empty, errorMessage) { }
        public APIErrorException(string message, ErrorMessage errorMessage) : base(message)
        {
            this.ErrorMessage = errorMessage;
        }
    }
}
