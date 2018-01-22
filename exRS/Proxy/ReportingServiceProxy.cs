using Common.Logging;
using exRS.Settings;
using exRS.SSRSService;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Services.Protocols;
using System.Xml.Linq;

namespace exRS.Proxy
{
    internal class ReportingServiceProxy : IReportingServiceProxy
    {
        private readonly ILog _log;
        private readonly ReportingService2010 _client;
        private readonly ISubscriptionSettingsFactory _subscriptionSettingsFactory;

        public ReportingServiceProxy(
            ILog log,
            ISubscriptionSettingsFactory subscriptionSettingsFactory,
            ReportingService2010 client)
        {
            _log = log;
            _client = client;
            _subscriptionSettingsFactory = subscriptionSettingsFactory;
        }

        public void DeleteSubscription(string subscriptionId) => _client.DeleteSubscription(subscriptionId);

        public string CreateSubscription(
            string path,
            string extensionName,
            (string Name, string Value)[] extensionParameters,
            string description,
            string eventType,
            string schedule,
            (string Name, string Value)[] parameters)
        {
            try
            {
                return _client.CreateSubscription(
                        path,
                        new ExtensionSettings
                        {
                            Extension = extensionName,
                            ParameterValues = extensionParameters.Select(x => new ParameterValue
                            {
                                Name = x.Name,
                                Value = x.Value,
                            }).ToArray(),
                        },
                        description,
                        eventType,
                        schedule,
                        parameters.Select(x => new ParameterValue
                        {
                            Name = x.Name,
                            Value = x.Value,
                        }).ToArray());
            }
            catch (SoapException ex) when (ex.Message.Contains("use a delivery extension that is not registered"))
            {
                _log.Error($"Extension '{extensionName}' is not supported by target SSRS server.");
            }

            return null;
        }

        public IEnumerable<string> GetAllSubscriptions(string reportPath) =>
            _client.ListSubscriptions(reportPath).Select(x => x.SubscriptionID);

        public void GetSubscriptionProperties(
            string subscriptionID,
            out string extensionName,
            out (string Name, string Value)[] extensionParameters,
            out string description,
            out string status,
            out string eventType,
            out string matchData,
            out (string Name, string Value)[] parameters)
        {
            _client.GetSubscriptionProperties(
                subscriptionID,
                out ExtensionSettings extSettings,
                out description,
                out ActiveState active,
                out status,
                out eventType,
                out matchData,
                out ParameterValue[] values);
            extensionName = extSettings.Extension;
            extensionParameters = extSettings.ParameterValues.Cast<ParameterValue>()
                .Select(x => (Name: x.Name, Value: x.Value)).ToArray();
            parameters = values.Select(x => (Name: x.Name, Value: x.Value)).ToArray();
        }

        public IEnumerable<ISubscriptionSettings> ListSubscriptions()
        {
            const string path = "/";
            _log.Trace($"{nameof(_client.ListSubscriptions)} for {path}");
            var subscriptions = _client.ListSubscriptions(path);

            foreach (var sub in subscriptions)
            {
                string desc = string.Empty;
                string eventType = string.Empty;
                string matchData = string.Empty;
                string status = string.Empty;
                _client.GetSubscriptionProperties(
                    sub.SubscriptionID,
                    out ExtensionSettings extSettings,
                    out desc,
                    out ActiveState active,
                    out status,
                    out eventType,
                    out matchData,
                    out ParameterValue[] values);

                var settings = _subscriptionSettingsFactory.GetSettings();
                settings.Path = sub.Path;
                settings.ExtensionName = sub.DeliverySettings.Extension;
                settings.ExtensionParameters = sub.DeliverySettings.ParameterValues.Cast<ParameterValue>()
                    .Select(x => (Name: x.Name, Value: x.Value)).ToList();
                settings.Description = sub.Description;
                settings.EventType = sub.EventType;
                settings.Parameters = values.Select(x => (Name: x.Name, Value: x.Value)).ToList();
                settings.SubscriptionId = sub.SubscriptionID;
                settings.ReportName = sub.Report;

                settings.ScheduleDefinition = matchData.Replace(
                    @"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?>", string.Empty).Replace(
                    @" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", string.Empty).Replace(
                    @" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", string.Empty).Replace(
                    @" xmlns=""http://schemas.microsoft.com/sqlserver/reporting/2010/03/01/ReportServer""", string.Empty);

                yield return settings;
            }
        }

        public bool PathExists(string path) =>
            _client.ListChildren(Path.GetDirectoryName(path).Replace("\\", "/"), false)
            .Any(x => x.Name == Path.GetFileName(path));

        public void CreateFolder(string name, string parentPath, (string Name, string Value)[] properties)
        {
            try
            {
                _client.CreateFolder(
                    name,
                    parentPath,
                    properties.Select(x => new Property { Name = x.Name, Value = x.Value }).ToArray());
            }
            catch (SoapException soapException) when (soapException.Message.Contains("AlreadyExists"))
            {
                _log.Info($"Folder {name} already exists");
            }
        }

        public string CreateReport(
            string name,
            string parentFolder,
            bool overwrite,
            byte[] definition)
        {
            try
            {
                return CreateCatalogItem("Report", name, parentFolder, overwrite, definition, null);
            }
            catch (SoapException soapException) when (soapException.Message.Contains("AlreadyExists"))
            {
                _log.Info($"The report {name} already exists");
            }
            catch (SoapException soapException) when (!soapException.Message.Contains("published"))
            {
                _log.Error(soapException.Message, soapException);
                throw;
            }

            return null;
        }

        public string CreateDataset(
            string name,
            string parentFolder,
            bool overwrite,
            byte[] definition)
        {
            try
            {
                return CreateCatalogItem("DataSet", name, parentFolder, overwrite, definition, null);
            }
            catch (SoapException soapException) when (soapException.Message.Contains("AlreadyExists"))
            {
                _log.Info($"The dataset {name} already exists");
            }
            catch (SoapException soapException) when (!soapException.Message.Contains("published"))
            {
                _log.Error(soapException.Message, soapException);
                throw;
            }

            return string.Empty;
        }

        public IEnumerable<string> GetItemDatasetReferences(string itemPath) =>
            GetItemReferences(itemPath, "DataSet");

        public IEnumerable<string> GetItemDatasourceReferences(string itemPath) =>
            GetItemReferences(itemPath, "Datasource");

        public IEnumerable<string> GetItemDataSourceReferences(string itemPath) =>
            GetItemReferences(itemPath, "DataSource");

        public void SetItemReferences(string itemPath, (string Name, string Reference)[] references)
        {
            _client.SetItemReferences(
                itemPath,
                references.Select(x => new ItemReference { Name = x.Name, Reference = x.Reference }).ToArray());
        }

        public string CreateDataSource(
            string name,
            string parent,
            bool overwrite,
            (string connectionString, string extension, string userName, string password, string promt) definition)
        {
            var dataSourceDefinition = new DataSourceDefinition
            {
                CredentialRetrieval = CredentialRetrievalEnum.Store,
                ConnectString = definition.connectionString,
                Enabled = true,
                EnabledSpecified = true,
                Extension = definition.extension,
                ImpersonateUser = false,
                ImpersonateUserSpecified = true,
                UserName = definition.userName,
                Password = definition.password,
            };
            if (!string.IsNullOrWhiteSpace(definition.promt))
            {
                dataSourceDefinition.Prompt = definition.promt;
            }

            dataSourceDefinition.WindowsCredentials = false;

            var result = string.Empty;
            try
            {
                var datasource = _client.CreateDataSource(name, parent, overwrite, dataSourceDefinition, null);
                result = datasource.Name;
            }
            catch (SoapException soapException) when (soapException.Message.Contains("AlreadyExists"))
            {
                _log.Info($"The dataset {name} already exists");
            }

            return result;
        }

        public string GetItemDefinition(string path)
        {
            var definition = _client.GetItemDefinition(path);
            using (var stream = new MemoryStream(definition))
            {
                return XDocument.Load(stream).ToString();
            }
        }

        public IEnumerable<string> ListReports() => ListItems("Report");

        public IEnumerable<string> ListDatasets() => ListItems("Dataset");

        public IEnumerable<string> ListDatasources() => ListItems("Datasource");

        public void CreateFolder(string name)
        {
            if (!PathExists(Path.Combine("/", name)))
            {
                _client.CreateFolder(name, "/", null);
            }
        }

        public void DeleteAllFolders()
        {
            var condition = new SearchCondition
            {
                Condition = ConditionEnum.Equals,
                ConditionSpecified = true,
                Name = "Type",
                Values = new[] { "Folder" },
            };
            var items = _client.FindItems("/", BooleanOperatorEnum.And, null, new SearchCondition[] { condition });
            foreach (var item in items.Where(x => x.Path != "/").OrderBy(x => x.Path.Length))
            {
                _client.DeleteItem(item.Path);
            }
        }

        private string CreateCatalogItem(
            string type,
            string name,
            string parentFolder,
            bool overwrite,
            byte[] definition,
            (string Name, string Value)[] properties)
        {
            var item = _client.CreateCatalogItem(
                type,
                name,
                parentFolder,
                overwrite,
                definition,
                properties?.Select(x => new Property { Name = x.Name, Value = x.Value }).ToArray(),
                out Warning[] warnings);
            if (warnings.Length > 0 && string.IsNullOrEmpty(item.Name))
            {
                foreach (var warning in warnings)
                {
                    _log.Warn(warning.Message);
                }
            }

            return item.Name;
        }

        private IEnumerable<string> GetItemReferences(string itemPath, string referenceItemType) =>
            _client.GetItemReferences(itemPath, referenceItemType).Select(x => x.Name);

        private IEnumerable<string> ListItems(string type)
        {
            var condition = new SearchCondition
            {
                Condition = ConditionEnum.Equals,
                ConditionSpecified = true,
                Name = "Type",
                Values = new[] { type },
            };
            var result = _client.FindItems("/", BooleanOperatorEnum.And, null, new SearchCondition[] { condition });

            foreach (var item in result)
            {
                yield return item.Path;
            }
        }
    }
}
