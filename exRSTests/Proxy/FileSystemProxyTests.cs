using Common.Logging;
using exRS.Proxy;
using FakeItEasy;
using Xunit;

namespace SSRSManagerTests.Proxy
{
    public class FileSystemProxyTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();

            var proxy = new FileSystemProxy(log);

            Assert.NotNull(proxy);
        }
    }
}
