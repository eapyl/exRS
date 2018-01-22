namespace exRS.Exceptions
{
    public class ReportNotFoundException : ExRSException
    {
        public ReportNotFoundException(string reportName)
            : base($"Can't find {reportName} at SSRS server. Please create a report first.")
        {
        }
    }
}
