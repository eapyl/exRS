using System.Collections.Generic;

namespace exRS.Settings
{
    internal interface ISubscriptionSettings
    {
        string Description { get; set; }

        string EventType { get; set; }

        string ExtensionName { get; set; }

        string FileName { get; }

        IList<(string Name, string Value)> ExtensionParameters { get; set; }

        IList<(string Name, string Value)> Parameters { get; set; }

        string Path { get; set; }

        string PostCreationSqlScript { get; set; }

        string ReportName { get; set; }

        string ScheduleDefinition { get; set; }

        string SubscriptionId { get; set; }

        ISubscriptionSettings Load(string path);

        void Save(string path);
    }
}
