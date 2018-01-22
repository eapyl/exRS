namespace exRS
{
    public interface IArguments
    {
        bool Report { get; }

        bool Subscription { get; }

        bool Recreate { get; }

        string FolderName { get; }

        string ReportName { get; }

        bool Backup { get; }

        bool Delete { get; }
    }
}
