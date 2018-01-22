using exRS.Exceptions;
using System;
using Xunit;

namespace SSRSManagerTests.Exceptions
{
    public class ExRSExceptionTests
    {
        [Fact]
        public void CreateWithMessage()
        {
            var message = nameof(ExRSExceptionTests);

            var exception = new ExRSException(message);

            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void CreateWithException()
        {
            var ex = new Exception();
            var message = nameof(ExRSExceptionTests);

            var exception = new ExRSException(message, ex);

            Assert.Equal(message, exception.Message);
            Assert.Equal(ex, exception.InnerException);
        }

        [Fact]
        public void Create()
        {
            var exception = new ExRSException();

            Assert.Equal("Exception of type 'exRS.Exceptions.ExRSException' was thrown.", exception.Message);
        }
    }
}
