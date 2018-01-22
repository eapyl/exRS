using Common.Logging;
using DryIoc;
using exRS.Managers;
using exRS.Proxy;
using exRS.Settings;
using exRS.SSRSService;
using System.Net;

namespace exRS
{
    public class Module
    {
        public static IContainer Install(IContainer container)
        {
            container.Register(made: Made.Of(() => LogManager.GetLogger<Module>()));
            container.Register<ISettings, ConfigurationSettings>(reuse: Reuse.Singleton);
            container.Register<IFileSystemProxy, FileSystemProxy>(reuse: Reuse.Singleton);
            container.Register<ISubscriptionSettingsFactory, SubscriptionSettingsFactory>(reuse: Reuse.Singleton);
            container.Register<IReportingServiceProxy, ReportingServiceProxy>(reuse: Reuse.Singleton);
            container.Register(made: Made.Of(() => ReportServiceFactory.Create(Arg.Of<ISettings>())), reuse: Reuse.Singleton);
            container.Register<ISubscriptionManager, SubscriptionManager>(reuse: Reuse.Singleton);
            container.Register<IManager, Manager>(reuse: Reuse.Singleton);
            container.Register<IReportManager, ReportManager>(reuse: Reuse.Singleton);
            return container;
        }

        internal static class ReportServiceFactory
        {
            public static ReportingService2010 Create(ISettings settings) =>
                new ReportingService2010
                {
                    Credentials = new NetworkCredential(settings.SSRSUser, settings.SSRSPassword),
                };
        }
    }
}
