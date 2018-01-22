using Common.Logging;
using exRS.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace exRS.Settings
{
    internal class SubscriptionSettings : ISubscriptionSettings
    {
        public const string FILENAME = "FILENAME";
        public const string ReportServerFileShare = "Report Server FileShare";
        private const string USERNAME = "USERNAME";
        private const string PASSWORD = "PASSWORD";
        private const string PATH = "PATH";
        private const string Parameter = "Parameter";
        private const string Name = "Name";
        private const string Value = "Value";
        private readonly ILog _log;
        private readonly IFileSystemProxy _fileSystem;
        private readonly ISettings _settings;
        private XElement _xmlDocument;

        public SubscriptionSettings(ILog log, ISettings settings, IFileSystemProxy fileSystem)
        {
            _log = log;
            _fileSystem = fileSystem;
            _settings = settings;
            _xmlDocument = new XElement(
                nameof(SubscriptionSettings),
                new XElement(nameof(ReportName)),
                new XElement(nameof(SubscriptionId)),
                new XElement(nameof(Path)),
                new XElement(nameof(ExtensionName)),
                new XElement(nameof(ExtensionParameters)),
                new XElement(nameof(Description)),
                new XElement(nameof(EventType)),
                new XElement(nameof(ScheduleDefinition)),
                new XElement(nameof(Parameters)),
                new XElement(nameof(PostCreationSqlScript)));
        }

        public string ReportName
        {
            get { return _xmlDocument.Element(nameof(ReportName)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(ReportName)).Value = value; }
        }

        public string SubscriptionId
        {
            get { return _xmlDocument.Element(nameof(SubscriptionId)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(SubscriptionId)).Value = value; }
        }

        public string Path
        {
            get { return _xmlDocument.Element(nameof(Path)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(Path)).Value = value; }
        }

        public string ExtensionName
        {
            get { return _xmlDocument.Element(nameof(ExtensionName)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(ExtensionName)).Value = value; }
        }

        public IList<(string Name, string Value)> ExtensionParameters
        {
            get
            {
                return
                    (from tuple in _xmlDocument.Element(nameof(ExtensionParameters)).Elements(Parameter)
                     select (Name: tuple.Element(Name).Value.Trim(), Value: tuple.Element(Value).Value.Trim())).ToList();
            }

            set
            {
                _xmlDocument.Element(nameof(ExtensionParameters)).ReplaceWith(
                    new XElement(
                        nameof(ExtensionParameters),
                        value.Select(x =>
                            new XElement(
                                Parameter,
                                new XElement(Name, x.Name),
                                new XElement(Value, x.Value)))
                        .ToArray()));
            }
        }

        public string Description
        {
            get { return _xmlDocument.Element(nameof(Description)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(Description)).Value = value; }
        }

        public string EventType
        {
            get { return _xmlDocument.Element(nameof(EventType)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(EventType)).Value = value; }
        }

        public string ScheduleDefinition
        {
            get { return _xmlDocument.Element(nameof(ScheduleDefinition)).ToString(); }
            set { _xmlDocument.Element(nameof(ScheduleDefinition)).ReplaceWith(XElement.Parse(value)); }
        }

        public string PostCreationSqlScript
        {
            get { return _xmlDocument.Element(nameof(PostCreationSqlScript)).Value.Trim(); }
            set { _xmlDocument.Element(nameof(PostCreationSqlScript)).Value = value; }
        }

        public string FileName => ExtensionParameters.Single(x => x.Name == FILENAME).Value;

        public IList<(string Name, string Value)> Parameters
        {
            get
            {
                return GetParameters().ToList();
            }

            set
            {
                _xmlDocument.Element(nameof(Parameters)).ReplaceWith(
                    new XElement(
                        nameof(Parameters),
                        value.Select(x =>
                            new XElement(
                                Parameter,
                                new XElement(Name, x.Name),
                                new XElement(Value, x.Value)))
                        .ToArray()));
            }
        }

        public ISubscriptionSettings Load(string path)
        {
            _log.Trace($"Load {nameof(SubscriptionSettings)} from {path}.");
            _xmlDocument = _fileSystem.LoadXElement(path);

            if (ExtensionName == ReportServerFileShare)
            {
                var copy = ExtensionParameters.ToArray();
                ReplaceValueUsingSettings(PATH, _settings.SubscriptionFileSharePath);
                ReplaceValueUsingSettings(USERNAME, _settings.SSRSUser);
                ReplaceValueUsingSettings(PASSWORD, _settings.SSRSPassword);

                ExtensionParameters = copy.ToList();

                void ReplaceValueUsingSettings(string name, string value)
                {
                    var item = copy.SingleOrDefault(x => x.Name == name);
                    var index = Array.IndexOf(copy, item);
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        copy[index].Value = value;
                    }
                }
            }

            return this;
        }

        public void Save(string path)
        {
            _log.Trace($"Save {nameof(SubscriptionSettings)} to {path}.");
            _fileSystem.SaveXElement(_xmlDocument, path);
        }

        private IEnumerable<(string Name, string Value)> GetParameters()
        {
            var elements = _xmlDocument.Element(nameof(Parameters)).Elements(Parameter);
            foreach (var element in elements)
            {
                yield return (
                    Name: element.Element(Name).Value.Trim(),
                    Value: element.Element(Value).Value.Trim());
            }
        }
    }
}
