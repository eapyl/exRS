namespace exRS
{
    internal interface ISettings
    {
        string SSRSPassword { get; }

        string BackupPath { get; }

        string SubscriptionSettingsFolder { get; }

        string SsrsUri { get; }

        string SSRSUser { get; }

        string DBUserForDataSource { get; }

        string DBPasswordForDataSource { get; }

        string SourceFolderPath { get; }

        string DataSourcesServerFolderName { get; }

        string DatasetsServerFolderName { get; }

        string SubscriptionFileSharePath { get; }
    }
}
