using Common.Logging;
using exRS.Managers;
using exRS.Proxy;

namespace exRS
{
    internal class Manager : IManager
    {
        private readonly IReportManager _reportManager;
        private readonly ISettings _settings;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILog _log;
        private readonly IReportingServiceProxy _reportingServiceProxy;

        public Manager(
            ISettings settings,
            ISubscriptionManager subscriptionManager,
            ILog log,
            IReportManager reportManager,
            IReportingServiceProxy reportingServiceProxy)
        {
            _reportManager = reportManager;
            _settings = settings;
            _subscriptionManager = subscriptionManager;
            _log = log;
            _reportingServiceProxy = reportingServiceProxy;
        }

        public void Execute(IArguments arguments)
        {
            if (arguments.Delete)
            {
                _log.Warn($"Deleting is running");
                _reportingServiceProxy.DeleteAllFolders();
            }

            if (arguments.Backup)
            {
                _reportManager.Backup(_settings.BackupPath);
                _subscriptionManager.Backup(_settings.BackupPath);
                return;
            }

            Run(arguments.ReportName, arguments.FolderName, arguments.Recreate);

            void Run(string reportName, string projectName, bool recreate)
            {
                if (arguments.Report)
                {
                    _reportManager.Deploy(
                        reportName,
                        projectName,
                        _settings.DatasetsServerFolderName,
                        _settings.DataSourcesServerFolderName,
                        recreate);
                }

                if (arguments.Subscription)
                {
                    _subscriptionManager.Deploy(reportName, recreate);
                }
            }
        }
    }
}
