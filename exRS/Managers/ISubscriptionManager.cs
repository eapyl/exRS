using exRS.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exRS.Managers
{
    internal interface ISubscriptionManager
    {
        ISubscriptionSettings Deploy(string reportName, bool recreate);

        string GetReportFileName(string subscriptionId);

        void Backup(string publishPath);

        string GetSubscriptionExtensionName(string subscriptionId);
    }
}
