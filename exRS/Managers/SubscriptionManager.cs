using Common.Logging;
using exRS.Exceptions;
using exRS.Proxy;
using exRS.Settings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace exRS.Managers
{
    internal class SubscriptionManager : ISubscriptionManager
    {
        private const string Subscriptions = "Subscriptions";
        private readonly ILog _log;
        private readonly IReportingServiceProxy _client;
        private readonly IFileSystemProxy _fileSystem;
        private readonly ISettings _settings;
        private readonly ISubscriptionSettingsFactory _subscriptionSettingsFactory;

        public SubscriptionManager(
            ILog log,
            IReportingServiceProxy client,
            ISubscriptionSettingsFactory subscriptionSettingsFactory,
            IFileSystemProxy fileSystem,
            ISettings settings)
        {
            _log = log;
            _client = client;
            _fileSystem = fileSystem;
            _settings = settings;
            _subscriptionSettingsFactory = subscriptionSettingsFactory;
        }

        public void Backup(string publishPath)
        {
            _log.Trace($"{nameof(Backup)}");
            var subscriptionSettings = _client.ListSubscriptions();

            var folder = Path.Combine(publishPath, Subscriptions);

            _fileSystem.CreateFolder(folder);

            foreach (var settings in subscriptionSettings)
            {
                settings.Save(Path.Combine(folder, $"{settings.ReportName}-{settings.SubscriptionId}"));
            }
        }

        public string GetReportFileName(string subscriptionId)
        {
            _log.Trace($"{nameof(GetReportFileName)} for {subscriptionId}");
            _client.GetSubscriptionProperties(
                subscriptionId,
                out string extName,
                out (string Name, string Value)[] extParameters,
                out string desc,
                out string status,
                out string eventType,
                out string matchData,
                out (string Name, string Value)[] values);

            return extParameters.Single(x => x.Name == SubscriptionSettings.FILENAME).Value;
        }

        public string GetSubscriptionExtensionName(string subscriptionId)
        {
            _log.Trace($"{nameof(GetSubscriptionExtensionName)} for {subscriptionId}");
            _client.GetSubscriptionProperties(
                subscriptionId,
                out string extName,
                out (string Name, string Value)[] extParameters,
                out string desc,
                out string status,
                out string eventType,
                out string matchData,
                out (string Name, string Value)[] values);

            return extName;
        }

        public ISubscriptionSettings Deploy(string reportName, bool recreate)
        {
            var pathToSubscriptionConfiguration = PathToSubscriptionConfiguration(_settings.SubscriptionSettingsFolder, reportName);

            _log.Info($"Processing {pathToSubscriptionConfiguration}");

            if (!_fileSystem.FileExists(pathToSubscriptionConfiguration))
            {
                throw new SubscriptionSettingsNotFoundException(pathToSubscriptionConfiguration);
            }

            var subscriptionSettings = _subscriptionSettingsFactory.GetSettings().Load(pathToSubscriptionConfiguration);

            if (!PathExists(subscriptionSettings.Path))
            {
                throw new ReportNotFoundException(subscriptionSettings.Path);
            }

            if (recreate)
            {
                var subscriptionIds = GetAllSubscriptions(subscriptionSettings.Path);
                foreach (var subscriptionId in subscriptionIds)
                {
                    DeleteSubscription(subscriptionId);
                }
            }

            var subId = CreateSubscription(subscriptionSettings);
            if (string.IsNullOrEmpty(subId))
            {
                return subscriptionSettings;
            }

            var fileName = $"{subscriptionSettings.ReportName}_{subscriptionSettings.SubscriptionId}";
            subscriptionSettings.Save(PathToSubscriptionConfiguration(_settings.SubscriptionSettingsFolder, fileName));

            return subscriptionSettings;
        }

        private bool PathExists(string reportPath) => _client.PathExists(reportPath);

        private void DeleteSubscription(string subscriptionId)
        {
            _log.Trace($"{nameof(DeleteSubscription)} {subscriptionId}");
            _client.DeleteSubscription(subscriptionId);
        }

        private IEnumerable<string> GetAllSubscriptions(string reportPath) => _client.GetAllSubscriptions(reportPath);

        private string CreateSubscription(ISubscriptionSettings subscriptionSettings)
        {
            _log.Trace($"{nameof(CreateSubscription)} for {subscriptionSettings.ReportName}");
            string subscriptionId = _client.CreateSubscription(
                    subscriptionSettings.Path,
                    subscriptionSettings.ExtensionName,
                    subscriptionSettings.ExtensionParameters.ToArray(),
                    subscriptionSettings.Description,
                    subscriptionSettings.EventType,
                    subscriptionSettings.ScheduleDefinition,
                    subscriptionSettings.Parameters.ToArray());

            if (string.IsNullOrEmpty(subscriptionId))
            {
                return subscriptionId;
            }

            _log.Trace($"New {nameof(subscriptionSettings.SubscriptionId)} is {subscriptionId}");
            return subscriptionId;
        }

        private string PathToSubscriptionConfiguration(string subscriptionSettingsFolder, string reportName) =>
            $"./{subscriptionSettingsFolder}/{reportName}.xml";
    }
}
