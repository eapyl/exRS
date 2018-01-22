namespace exRS.Managers
{
    internal interface IReportManager
    {
        void Deploy(string reportName, string reportFolder, string datasetFolder, string dataSourceFolder, bool recreate);

        void Backup(string publishPath);
    }
}
