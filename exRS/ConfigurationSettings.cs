using static exRS.Properties.Settings;

namespace exRS
{
    internal class ConfigurationSettings : ISettings
    {
        public string SsrsUri => Default.exRS_SSRSService_ReportingService2010;

        public string SSRSUser => Default.SSRSUser;

        public string SSRSPassword => Default.SSRSPassword;

        public string BackupPath => Default.backupPath;

        public string SubscriptionSettingsFolder => Default.subscriptionSettingsFolder;

        public string DBUserForDataSource => Default.dbUserForDataSource;

        public string DBPasswordForDataSource => Default.dbPasswordForDataSource;

        public string SourceFolderPath => Default.sourceFolderPath;

        public string DataSourcesServerFolderName => Default.datasourcesServerFolderName;

        public string DatasetsServerFolderName => Default.datasetsServerFolderName;

        public string SubscriptionFileSharePath => Default.subscriptionFileSharePath;
    }
}
