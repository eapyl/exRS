using exRS.Exceptions;
using Xunit;

namespace SSRSManagerTests.Exceptions
{
    public class SubscriptionSettingsNotFoundExceptionTests
    {
        [Fact]
        public void Create()
        {
            var message = nameof(Create);

            var exception = new SubscriptionSettingsNotFoundException(message);

            Assert.Equal($"Can't find settings {message}. Please create subscription settings for this report first or remove -s parameter for this report.", exception.Message);
        }
    }
}
