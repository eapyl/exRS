using Common.Logging;
using exRS;
using exRS.Managers;
using exRS.Proxy;
using exRS.Settings;
using FakeItEasy;
using Xunit;

namespace SSRSManagerTests
{
    public class ManagerTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var arguments = A.Fake<IArguments>();
            var subscriptionManager = A.Fake<ISubscriptionManager>();
            var reportManager = A.Fake<IReportManager>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            var manager = new Manager(settings, subscriptionManager, log,
                reportManager, reportingServiceProxy);

            Assert.NotNull(manager);
        }

        [Fact]
        public void DeleteAll()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var arguments = A.Fake<IArguments>();
            var subscriptionManager = A.Fake<ISubscriptionManager>();
            var reportManager = A.Fake<IReportManager>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => arguments.Delete).Returns(true);

            var manager = new Manager(settings, subscriptionManager, log,
                reportManager, reportingServiceProxy);

            manager.Execute(arguments);

            A.CallTo(() => reportingServiceProxy.DeleteAllFolders()).MustHaveHappened();
        }

        [Fact]
        public void Backup()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var arguments = A.Fake<IArguments>();
            var subscriptionManager = A.Fake<ISubscriptionManager>();
            var reportManager = A.Fake<IReportManager>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => arguments.Backup).Returns(true);

            var manager = new Manager(settings, subscriptionManager, log,
                reportManager, reportingServiceProxy);

            manager.Execute(arguments);

            A.CallTo(() => subscriptionManager.Backup(A<string>._)).MustHaveHappened();
            A.CallTo(() => reportManager.Backup(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void Deploy()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var arguments = A.Fake<IArguments>();
            var subscriptionManager = A.Fake<ISubscriptionManager>();
            var reportManager = A.Fake<IReportManager>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => arguments.Backup).Returns(false);
            A.CallTo(() => arguments.Subscription).Returns(false);
            A.CallTo(() => arguments.Report).Returns(true);
            A.CallTo(() => arguments.ReportName).Returns(nameof(arguments.ReportName));

            var manager = new Manager(settings, subscriptionManager, log,
                reportManager, reportingServiceProxy);

            manager.Execute(arguments);

            A.CallTo(() => reportManager.Deploy(A<string>._, A<string>._, A<string>._, A<string>._, A<bool>._)).MustHaveHappened();
        }

        [Fact]
        public void Subscription()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var arguments = A.Fake<IArguments>();
            var subscriptionManager = A.Fake<ISubscriptionManager>();
            var reportManager = A.Fake<IReportManager>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => arguments.Backup).Returns(false);
            A.CallTo(() => arguments.Subscription).Returns(true);
            A.CallTo(() => arguments.Report).Returns(false);
            A.CallTo(() => arguments.ReportName).Returns(nameof(arguments.ReportName));

            var manager = new Manager(settings, subscriptionManager, log,
                reportManager, reportingServiceProxy);

            manager.Execute(arguments);

            A.CallTo(() => subscriptionManager.Deploy(A<string>._, A<bool>._)).MustHaveHappened();
        }
    }
}
