using Common.Logging;
using exRS;
using exRS.Proxy;
using exRS.Settings;
using FakeItEasy;
using Xunit;

namespace SSRSManagerTests.Settings
{
    public class SubscriptionSettingsFactoryTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var factory = new SubscriptionSettingsFactory(log, settings, fileSystemProxy);

            Assert.NotNull(factory);
        }

        [Fact]
        public void GetSettings()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();

            var factory = new SubscriptionSettingsFactory(log, settings, fileSystemProxy);

            var instance = factory.GetSettings();

            Assert.NotNull(instance);
        }
    }
}
