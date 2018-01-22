using exRS.Exceptions;
using Xunit;

namespace SSRSManagerTests.Exceptions
{
    public class ReportNotFoundExceptionTests
    {
        [Fact]
        public void Create()
        {
            var message = nameof(Create);

            var exception = new ReportNotFoundException(message);

            Assert.Equal($"Can't find {message} at SSRS server. Please create a report first.", exception.Message);
        }
    }
}
