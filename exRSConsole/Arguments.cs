using exRS;

namespace exRSConsole
{
    public class Arguments : IArguments
    {
        public bool Report { get; set; }

        public bool Subscription { get; set; }

        public bool Recreate { get; set; }

        public string FolderName { get; set; }

        public string ReportName { get; set; }

        public bool Backup { get; set; }

        public bool Delete { get; set; }
    }
}
