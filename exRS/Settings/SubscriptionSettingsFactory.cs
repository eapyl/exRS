using Common.Logging;
using exRS.Proxy;

namespace exRS.Settings
{
    internal class SubscriptionSettingsFactory : ISubscriptionSettingsFactory
    {
        private readonly ILog _log;
        private readonly IFileSystemProxy _fileSystem;
        private readonly ISettings _settings;

        public SubscriptionSettingsFactory(
            ILog log,
            ISettings settings,
            IFileSystemProxy fileSystem)
        {
            _log = log;
            _fileSystem = fileSystem;
            _settings = settings;
        }

        public ISubscriptionSettings GetSettings() => new SubscriptionSettings(_log, _settings, _fileSystem);
    }
}
