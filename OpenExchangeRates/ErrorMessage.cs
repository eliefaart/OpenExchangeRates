using System;

namespace OpenExchangeRates
{
    public class ErrorMessage
    {
        public bool Error { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return "[" + Status + "] " + Message + "\n" + Description;
        }
    }
}
