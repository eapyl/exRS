using exRS.Settings;
using System.Collections.Generic;

namespace exRS.Proxy
{
    internal interface IReportingServiceProxy
    {
        string CreateSubscription(
            string path,
            string extensionName,
            (string Name, string Value)[] extensionParameters,
            string description,
            string eventType,
            string schedule,
            (string Name, string Value)[] parameters);

        void DeleteSubscription(string subscriptionId);

        void GetSubscriptionProperties(
            string subscriptionID,
            out string extensionName,
            out (string Name, string Value)[] extensionParameters,
            out string description,
            out string status,
            out string eventType,
            out string matchData,
            out (string Name, string Value)[] parameters);

        IEnumerable<ISubscriptionSettings> ListSubscriptions();

        IEnumerable<string> GetAllSubscriptions(string reportPath);

        bool PathExists(string path);

        void CreateFolder(string name, string parentPath, (string Name, string Value)[] properties);

        string CreateReport(string name, string parentFolder, bool overwrite, byte[] definition);

        IEnumerable<string> GetItemDatasetReferences(string reportPath);

        string CreateDataset(string name, string parentFolder, bool overwrite, byte[] definition);

        void SetItemReferences(string itemPath, (string Name, string Reference)[] references);

        string CreateDataSource(
            string name,
            string parent,
            bool overwrite,
            (string connectionString, string extension, string userName, string password, string promt) definition);

        IEnumerable<string> ListReports();

        IEnumerable<string> ListDatasets();

        IEnumerable<string> ListDatasources();

        void CreateFolder(string name);

        string GetItemDefinition(string path);

        IEnumerable<string> GetItemDatasourceReferences(string itemPath);

        IEnumerable<string> GetItemDataSourceReferences(string itemPath);

        void DeleteAllFolders();
    }
}
