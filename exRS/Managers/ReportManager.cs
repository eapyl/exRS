using Common.Logging;
using exRS.Proxy;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace exRS.Managers
{
    internal class ReportManager : IReportManager
    {
        public const string REPORTEXTENSION = ".rdl";
        public const string DATASETEXTENSION = ".rsd";
        public const string DATASOURCEEXTENSION = ".rds";

        private const string SharedDataSetReference = "SharedDataSetReference";
        private const string DataSourceReference = "DataSourceReference";
        private const string ConnectString = "ConnectString";
        private const string Extension = "Extension";
        private const string DataSourceID = "DataSourceID";
        private const string Prompt = "Prompt";
        private const string DataSetReference = "<DataSet Name=\"{0}\">";
        private const string DatasourceReference = "<DataSource Name=\"{0}\">";

        private readonly ILog _log;
        private readonly IFileSystemProxy _fileSystemProxy;
        private readonly ISettings _settings;
        private readonly IReportingServiceProxy _reportingServiceProxy;

        public ReportManager(
            ILog log,
            ISettings settings,
            IFileSystemProxy fileSystemProxy,
            IReportingServiceProxy reportingServiceProxy)
        {
            _log = log;
            _fileSystemProxy = fileSystemProxy;
            _settings = settings;
            _reportingServiceProxy = reportingServiceProxy;
        }

        public void Backup(string publishPath)
        {
            _log.Info("Backuping reports.");
            BackupType(_reportingServiceProxy.ListReports(), REPORTEXTENSION);
            _log.Info("Backuping datasets.");
            BackupType(_reportingServiceProxy.ListDatasets(), DATASETEXTENSION);
            _log.Info("Backuping datasources.");
            BackupType(_reportingServiceProxy.ListDatasources(), DATASOURCEEXTENSION);

            void CheckFolderNames(string name, string extension)
            {
                switch (extension)
                {
                    case DATASETEXTENSION:
                        if (name != "Datasets")
                        {
                            _log.Warn($"All datasets should be in 'Datasets' folder, not in {name}");
                        }

                        break;
                    case DATASOURCEEXTENSION:
                        if (name != "Data Sources")
                        {
                            _log.Warn($"All datasources should be in 'Data Sources' folder, not in {name}");
                        }

                        break;
                }
            }

            void BackupType(IEnumerable<string> items, string extension) =>
                Parallel.ForEach(items, (item) =>
                {
                    var directory = Path.GetFileName(Path.GetDirectoryName(item));
                    _fileSystemProxy.CreateFolder(Path.Combine(publishPath, directory));
                    var reportName = Path.GetFileName(item);
                    var description = _reportingServiceProxy.GetItemDefinition(item);
                    CheckFolderNames(directory, extension);
                    _fileSystemProxy.WriteAllText(Path.Combine(publishPath, directory, reportName + extension), description);
                });
        }

        public void Deploy(
            string reportName,
            string reportFolder,
            string datasetFolder,
            string datasourceFolder,
            bool recreate)
        {
            _log.Info($"Processing report {reportName} in project {reportFolder}.");

            var folderWithFiles = reportFolder;

            // create report on SSRS server
            DeployReport(reportName, reportFolder, folderWithFiles, recreate);

            var reportDefinition = _fileSystemProxy.ReadAllText(GetFullPathToReport(reportName, folderWithFiles));

            var reportPathOnServer = $"/{reportFolder}/{reportName}";

            // read all dataset which are in created report
            foreach (var dataSetReference in
                _reportingServiceProxy.GetItemDatasetReferences(reportPathOnServer)
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var sharedReference = SharedDatasetReference(dataSetReference);

                if (sharedReference == null)
                {
                    continue;
                }

                // create dataset on SSRS server
                DeployDataset(sharedReference, datasetFolder, folderWithFiles, recreate);

                _log.Trace($"Linking report {reportName} and dataset {sharedReference}");
                var datasetPathOnServer = $"/{datasetFolder}/{sharedReference}";

                // link created report with created dataset
                _reportingServiceProxy.SetItemReferences(reportPathOnServer, new[] { (dataSetReference, datasetPathOnServer) });

                var datasetDefinition = _fileSystemProxy.ReadAllText(GetFullPathToDataset(sharedReference, folderWithFiles));

                var datasourceReference = GetValueFromDefinition(datasetDefinition, DataSourceReference);

                // create data source from created dataset on SSRS server
                DeployDatasource(datasourceReference, datasourceFolder, folderWithFiles, recreate);

                var dataSourceName = _reportingServiceProxy.GetItemDatasourceReferences(datasetPathOnServer).Single();

                _log.Trace($"Linking dataset {sharedReference} and datasource {datasourceReference}");

                // link created data source and created dataset
                _reportingServiceProxy.SetItemReferences(datasetPathOnServer, new[] { (dataSourceName, $"/{datasourceFolder}/{datasourceReference}") });
            }

            // read all data sources which are in created report
            foreach (var datasourceReference in
                _reportingServiceProxy.GetItemDataSourceReferences(reportPathOnServer)
                .Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var sharedReference = SharedDatasourceReference(datasourceReference);

                if (sharedReference == null)
                {
                    continue;
                }

                // create datasource on SSRS server
                DeployDatasource(sharedReference, datasourceFolder, folderWithFiles, recreate);

                _log.Trace($"Linking report {reportName} and datasource {sharedReference}");
                var datasourcePathOnServer = $"/{datasourceFolder}/{sharedReference}";

                // link created data source and created report
                _reportingServiceProxy.SetItemReferences(reportPathOnServer, new[] { (datasourceReference, datasourcePathOnServer) });
            }

            string SharedDatasetReference(string datasetName)
            {
                var index = reportDefinition.IndexOf(string.Format(DataSetReference, datasetName));
                if (index > -1)
                {
                    return GetValueFromDefinition(reportDefinition, SharedDataSetReference, index);
                }

                return null;
            }

            string SharedDatasourceReference(string datasourceName)
            {
                var index = reportDefinition.IndexOf(string.Format(DatasourceReference, datasourceName));
                if (index > -1)
                {
                    return GetValueFromDefinition(reportDefinition, DataSourceReference, index);
                }

                return null;
            }
        }

        private void DeployReport(string reportName, string reportFolder, string folderWithFile, bool recreate)
        {
            var definition = _fileSystemProxy.ReadAllBytes(GetFullPathToReport(reportName, folderWithFile));

            _log.Trace($"Creating report {reportName} in {reportFolder} folder");
            _reportingServiceProxy.CreateFolder(reportFolder);
            _reportingServiceProxy.CreateReport(reportName, $"/{reportFolder}", recreate, definition);
        }

        private void DeployDataset(string datasetName, string datasetFolder, string folderWithFile, bool recreate)
        {
            var definition = _fileSystemProxy.ReadAllBytes(GetFullPathToDataset(datasetName, folderWithFile));

            _log.Trace($"Creating dataset {datasetName}");
            _reportingServiceProxy.CreateFolder(datasetFolder);
            _reportingServiceProxy.CreateDataset(datasetName, $"/{datasetFolder}", recreate, definition);
        }

        private void DeployDatasource(string datasourceName, string datasourceFolder, string folderWithFile, bool recreate)
        {
            var datasourceDefinition = _fileSystemProxy.ReadAllText(GetFullPathToDatasource(datasourceName, folderWithFile));

            var datasourceInternalDefinition = (
                connectionString: GetValueFromDefinition(datasourceDefinition, ConnectString),
                extension: GetValueFromDefinition(datasourceDefinition, Extension),
                userName: _settings.DBUserForDataSource,
                password: _settings.DBPasswordForDataSource,
                promt: GetValueFromDefinition(datasourceDefinition, Prompt));

            _log.Trace($"Creating datasource {datasourceName}");
            _reportingServiceProxy.CreateFolder(datasourceFolder);
            _reportingServiceProxy.CreateDataSource(
                datasourceName,
                Path.Combine("/", datasourceFolder),
                recreate,
                datasourceInternalDefinition);
        }

        private string GetValueFromDefinition(string text, string element, int startIndex = 0)
        {
            if (!text.Contains($"<{element}>"))
            {
                return null;
            }

            var start = text.IndexOf($"<{element}>", startIndex) + element.Length + 2;
            var end = text.IndexOf($"</{element}>", startIndex);
            return text.Substring(start, end - start);
        }

        private string GetFullPathToDatasource(string datasourceName, string datasourceFolder) =>
            Path.Combine(_settings.SourceFolderPath, datasourceFolder, datasourceName + DATASOURCEEXTENSION);

        private string GetFullPathToDataset(string datasetName, string datasetFolder) =>
            Path.Combine(_settings.SourceFolderPath, datasetFolder, datasetName + DATASETEXTENSION);

        private string GetFullPathToReport(string reportName, string reportFolder) =>
            Path.Combine(_settings.SourceFolderPath, reportFolder, reportName + REPORTEXTENSION);
    }
}
