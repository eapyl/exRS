using Common.Logging;
using exRS;
using exRS.Managers;
using exRS.Proxy;
using FakeItEasy;
using System.Linq;
using Xunit;

namespace SSRSManagerTests.Managers
{
    public class ReportManagerTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            Assert.NotNull(reportManager);
        }

        [Fact]
        public void Backup()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.ListReports()).Returns(new[] { "/Report/Report"});
            A.CallTo(() => reportingServiceProxy.ListDatasets()).Returns(new[] { "/Dataset/Dataset" });
            A.CallTo(() => reportingServiceProxy.ListDatasources()).Returns(new[] { "/DataSource/DataSource" });

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Backup(string.Empty);

            A.CallTo(() => log.Warn("All datasets should be in 'Datasets' folder, not in Dataset")).MustHaveHappened();
            A.CallTo(() => log.Warn("All datasources should be in 'Data Sources' folder, not in DataSource")).MustHaveHappened();
            A.CallTo(() => fileSystemProxy.WriteAllText(A<string>._, A<string>._)).MustHaveHappened(Repeated.Exactly.Times(3));
            Assert.NotNull(reportManager);
        }

        [Fact]
        public void BackupWithoutWarnings()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.ListReports()).Returns(new[] { "/Report/Report" });
            A.CallTo(() => reportingServiceProxy.ListDatasets()).Returns(new[] { "/Datasets/Dataset" });
            A.CallTo(() => reportingServiceProxy.ListDatasources()).Returns(new[] { "/Data Sources/DataSource" });

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Backup(string.Empty);

            A.CallTo(() => log.Warn(A<string>._)).MustNotHaveHappened();
            A.CallTo(() => fileSystemProxy.WriteAllText(A<string>._, A<string>._)).MustHaveHappened(Repeated.Exactly.Times(3));
            Assert.NotNull(reportManager);
        }

        [Fact]
        public void Deploy()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.GetItemDatasetReferences(A<string>._)).Returns(new[] { "DatasetReference" });
            A.CallTo(() => reportingServiceProxy.GetItemDatasourceReferences(A<string>._)).Returns(new[] { "dataSourceName" });
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("Report"))).Returns("<DataSet Name=\"DatasetReference\"><SharedDataSetReference>sharedReference</SharedDataSetReference>");
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("sharedReference"))).Returns("<DataSourceReference>datasourceReference</DataSourceReference>");

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Deploy("Report", "Report", "Datasets", "Data Sources", false);

            A.CallTo(() => reportingServiceProxy.CreateFolder("Report")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateReport("Report", "/Report", false, A<byte[]>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Datasets")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataset("sharedReference", "/Datasets", false, A<byte[]>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Data Sources")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataSource("datasourceReference", "/Data Sources", false, A<(string, string, string, string, string)>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("sharedReference"), A<(string, string)[]>._)).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("Report"), A<(string, string)[]>._)).MustHaveHappened();
        }

        [Fact]
        public void DeployWithEmbeddedDatasource()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.GetItemDataSourceReferences(A<string>._)).Returns(new[] { "dataSourceName" });
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("Report"))).Returns("<DataSource Name=\"dataSourceName\"><DataSourceReference>sharedReference</DataSourceReference>");

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Deploy("Report", "Report", "Datasets", "Data Sources", false);

            A.CallTo(() => reportingServiceProxy.CreateFolder("Report")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateReport("Report", "/Report", false, A<byte[]>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Datasets")).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataset("sharedReference", "/Datasets", false, A<byte[]>._)).MustNotHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Data Sources")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataSource("sharedReference", "/Data Sources", false, A<(string, string, string, string, string)>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("sharedReference"), A<(string, string)[]>._)).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("Report"), A<(string, string)[]>._)).MustHaveHappened();
        }

        [Fact]
        public void DeployWithEmbeddedDatasourceButWithoutReference()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.GetItemDataSourceReferences(A<string>._)).Returns(new[] { "dataSourceName" });
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("Report"))).Returns("");

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Deploy("Report", "Report", "Datasets", "Data Sources", false);

            A.CallTo(() => reportingServiceProxy.CreateFolder("Report")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateReport("Report", "/Report", false, A<byte[]>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Datasets")).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataset("sharedReference", "/Datasets", false, A<byte[]>._)).MustNotHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Data Sources")).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataSource("sharedReference", "/Data Sources", false, A<(string, string, string, string, string)>._)).MustNotHaveHappened();

            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("sharedReference"), A<(string, string)[]>._)).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("Report"), A<(string, string)[]>._)).MustNotHaveHappened();
        }

        [Fact]
        public void DeployWithoutSharedDataset()
        {
            var log = A.Fake<ILog>();
            var fileSystemProxy = A.Fake<IFileSystemProxy>();
            var settings = A.Fake<ISettings>();
            var reportingServiceProxy = A.Fake<IReportingServiceProxy>();

            A.CallTo(() => reportingServiceProxy.GetItemDatasetReferences(A<string>._)).Returns(new[] { "DatasetReference" });
            A.CallTo(() => reportingServiceProxy.GetItemDatasourceReferences(A<string>._)).Returns(new[] { "dataSourceName" });
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("Report"))).Returns("<Data Name=\"DatasetReference\"><Shared>sharedReference</Shared>");
            A.CallTo(() => fileSystemProxy.ReadAllText(A<string>.That.Contains("sharedReference"))).Returns("<DataSourceReference>datasourceReference</DataSourceReference>");

            var reportManager = new ReportManager(log, settings, fileSystemProxy, reportingServiceProxy);

            reportManager.Deploy("Report", "Report", "Datasets", "Data Sources", false);

            A.CallTo(() => reportingServiceProxy.CreateFolder("Report")).MustHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateReport("Report", "/Report", false, A<byte[]>._)).MustHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Datasets")).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataset("sharedReference", "/Datasets", false, A<byte[]>._)).MustNotHaveHappened();

            A.CallTo(() => reportingServiceProxy.CreateFolder("Data Sources")).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.CreateDataSource("datasourceReference", "/Data Sources", false, A<(string, string, string, string, string)>._)).MustNotHaveHappened();

            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("sharedReference"), A<(string, string)[]>._)).MustNotHaveHappened();
            A.CallTo(() => reportingServiceProxy.SetItemReferences(A<string>.That.Contains("Report"), A<(string, string)[]>._)).MustNotHaveHappened();
        }
    }
}
