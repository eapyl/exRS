using System;

namespace exRS.Exceptions
{
    public class ExRSException : Exception
    {
        public ExRSException()
        {
        }

        public ExRSException(string message)
            : base(message)
        {
        }

        public ExRSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
