using Common.Logging;
using exRS;
using exRS.Exceptions;
using exRS.Managers;
using exRS.Proxy;
using exRS.Settings;
using FakeItEasy;
using Xunit;

namespace SSRSManagerTests.Managers
{
    public class SubscriptionManagerTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            Assert.NotNull(manager);
        }

        [Fact]
        public void Backup()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSetting = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => reportingService.ListSubscriptions()).Returns(new[] { subSetting });

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            manager.Backup(string.Empty);

            A.CallTo(() => subSetting.Save(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void Deploy()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSettings = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => subscriptionSettingsFactory.GetSettings()).Returns(subSettings);
            A.CallTo(() => reportingService.PathExists(A<string>._)).Returns(true);
            A.CallTo(() => subSettings.Load(A<string>._)).Returns(subSettings);
            A.CallTo(() => subSettings.PostCreationSqlScript).Returns(nameof(subSettings.PostCreationSqlScript));
            A.CallTo(() => fileSystem.FileExists(A<string>._)).Returns(true);
            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).Returns(nameof(Deploy));

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            manager.Deploy(string.Empty, false);

            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).MustHaveHappened();
            A.CallTo(() => subSettings.Save(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void DeploySubscriptionWithError()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSettings = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => subscriptionSettingsFactory.GetSettings()).Returns(subSettings);
            A.CallTo(() => reportingService.PathExists(A<string>._)).Returns(true);
            A.CallTo(() => subSettings.Load(A<string>._)).Returns(subSettings);
            A.CallTo(() => subSettings.PostCreationSqlScript).Returns(nameof(subSettings.PostCreationSqlScript));
            A.CallTo(() => fileSystem.FileExists(A<string>._)).Returns(true);
            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).Returns(null);

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            manager.Deploy(string.Empty, false);

            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).MustHaveHappened();
            A.CallTo(() => subSettings.Save(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void DeployForNonExistingReport()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSettings = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => subscriptionSettingsFactory.GetSettings()).Returns(subSettings);
            A.CallTo(() => reportingService.PathExists(A<string>._)).Returns(false);
            A.CallTo(() => subSettings.Load(A<string>._)).Returns(subSettings);
            A.CallTo(() => subSettings.PostCreationSqlScript).Returns(nameof(subSettings.PostCreationSqlScript));
            A.CallTo(() => fileSystem.FileExists(A<string>._)).Returns(true);

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            Assert.Throws<ReportNotFoundException>(() => manager.Deploy(string.Empty, false));

            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).MustNotHaveHappened();
            A.CallTo(() => subSettings.Save(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void DeployForNonExistingSubsciptionSettings()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSettings = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => fileSystem.FileExists(A<string>._)).Returns(false);

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            Assert.Throws<SubscriptionSettingsNotFoundException>(() => manager.Deploy(string.Empty, false));

            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).MustNotHaveHappened();
            A.CallTo(() => subSettings.Save(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void DeployWithRecreatingSubscription()
        {
            var log = A.Fake<ILog>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();

            var subSettings = A.Fake<ISubscriptionSettings>();
            A.CallTo(() => subscriptionSettingsFactory.GetSettings()).Returns(subSettings);
            A.CallTo(() => reportingService.PathExists(A<string>._)).Returns(true);
            A.CallTo(() => subSettings.Load(A<string>._)).Returns(subSettings);
            A.CallTo(() => subSettings.PostCreationSqlScript).Returns(nameof(subSettings.PostCreationSqlScript));
            A.CallTo(() => reportingService.GetAllSubscriptions(A<string>._)).Returns(new[] { string.Empty });
            A.CallTo(() => fileSystem.FileExists(A<string>._)).Returns(true);
            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).Returns(nameof(DeployWithRecreatingSubscription));

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, settings);

            manager.Deploy(string.Empty, true);

            A.CallTo(() => log.Error(A<object>._)).MustNotHaveHappened();
            A.CallTo(() => reportingService.DeleteSubscription(A<string>._)).MustHaveHappened();
            A.CallTo(() => reportingService.CreateSubscription(A<string>._, A<string>._, A<(string, string)[]>._,
                A<string>._, A<string>._, A<string>._, A<(string, string)[]>._)).MustHaveHappened();
            A.CallTo(() => subSettings.Save(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void GetReportFileName()
        {
            var log = A.Fake<ILog>();
            var mainSettings = A.Fake<ISettings>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            string extName;
            (string Name, string Value)[] extParameters;
            (string Name, string Value)[] values;
            string desc;
            string eventType;
            string matchData;
            string status;
            A.CallTo(() => reportingService.GetSubscriptionProperties(A<string>._, out extName, out extParameters,
                out desc, out status, out eventType, out matchData, out values))
            .AssignsOutAndRefParameters(string.Empty, new[] { (Name: "FILENAME", VALUE: nameof(GetReportFileName)) }, string.Empty,
                string.Empty, string.Empty, string.Empty, null);

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, mainSettings);

            var reportName = manager.GetReportFileName(string.Empty);

            A.CallTo(() => log.Trace(A<string>._)).MustHaveHappened();
            Assert.Equal(nameof(GetReportFileName), reportName);
        }

        [Fact]
        public void GetExtensionName()
        {
            var log = A.Fake<ILog>();
            var mainSettings = A.Fake<ISettings>();
            var reportingService = A.Fake<IReportingServiceProxy>();
            var subscriptionSettingsFactory = A.Fake<ISubscriptionSettingsFactory>();
            var fileSystem = A.Fake<IFileSystemProxy>();
            string extName = null;
            (string Name, string Value)[] extParameters;
            (string Name, string Value)[] values;
            string desc;
            string eventType;
            string matchData;
            string status;
            A.CallTo(() => reportingService.GetSubscriptionProperties(A<string>._, out extName, out extParameters,
                out desc, out status, out eventType, out matchData, out values))
            .AssignsOutAndRefParameters(nameof(GetExtensionName), null, string.Empty,
                string.Empty, string.Empty, string.Empty, null);

            var manager = new SubscriptionManager(log, reportingService, subscriptionSettingsFactory, fileSystem, mainSettings);

            var extensionName = manager.GetSubscriptionExtensionName(string.Empty);

            A.CallTo(() => log.Trace(A<string>._)).MustHaveHappened();
            Assert.Equal(nameof(GetExtensionName), extensionName);
        }
    }
}
