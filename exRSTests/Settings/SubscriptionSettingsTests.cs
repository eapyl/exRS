using Common.Logging;
using exRS;
using exRS.Proxy;
using exRS.Settings;
using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace SSRSManagerTests.Settings
{
    public class SubscriptionSettingsTests
    {
        [Fact]
        public void Create()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            Assert.NotNull(instance);
        }

        [Fact]
        public void CheckFileName()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            var parameters = new List<(string Name, string Value)>
            {
                (Name: SubscriptionSettings.FILENAME, Value: SubscriptionSettings.FILENAME)
            };

            instance.ExtensionParameters = parameters;

            var result = instance.FileName;

            Assert.Equal(SubscriptionSettings.FILENAME, result);
        }

        [Theory]
        [InlineData(nameof(SubscriptionSettings.ReportName))]
        [InlineData(nameof(SubscriptionSettings.SubscriptionId))]
        [InlineData(nameof(SubscriptionSettings.Path))]
        [InlineData(nameof(SubscriptionSettings.ExtensionName))]
        [InlineData(nameof(SubscriptionSettings.Description))]
        [InlineData(nameof(SubscriptionSettings.EventType))]
        [InlineData(nameof(SubscriptionSettings.PostCreationSqlScript))]
        public void CheckProperty(string propertyName)
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            instance.GetType().GetProperty(propertyName).SetValue(instance, propertyName);

            Assert.Equal(propertyName, instance.GetType().GetProperty(propertyName).GetValue(instance));
        }

        [Fact]
        public void CheckExtensionParameters()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            var parameters = new List<(string Name, string Value)>
            {
                (Name: "1", Value: "val1"),
                (Name: "2", Value: "val2")
            };

            instance.ExtensionParameters = parameters;

            Assert.Equal(parameters, instance.ExtensionParameters);
        }

        [Fact]
        public void CheckScheduleDefinition()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            var schedule = @"<ScheduleDefinition>
  <StartDateTime>2017-11-03T00:00:00.000</StartDateTime>
  <MonthlyDOWRecurrence>
    <WhichWeek>FirstWeek</WhichWeek>
    <DaysOfWeek>
      <Monday>true</Monday>
    </DaysOfWeek>
    <MonthsOfYear>
      <January>true</January>
    </MonthsOfYear>
  </MonthlyDOWRecurrence>
</ScheduleDefinition>";

            instance.ScheduleDefinition = schedule;

            Assert.Equal(schedule, instance.ScheduleDefinition);
        }

        [Fact]
        public void CheckParameters()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            var parameters = new List<(string Name, string Value)>
            {
                (Name: "1", Value: "val1"),
                (Name: "2", Value: "val2")
            };

            instance.Parameters = parameters;

            Assert.Equal(parameters, instance.Parameters);
        }

        [Fact]
        public void Load()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            A.CallTo(() => fileSystem.LoadXElement(string.Empty)).Returns(XElement.Parse(@"
<SubscriptionSettings>
  <ExtensionName />
</SubscriptionSettings>"));

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            instance.Load(string.Empty);

            A.CallTo(() => fileSystem.LoadXElement(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void LoadAndReplaceValueUsingSettings()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            A.CallTo(() => settings.SubscriptionFileSharePath).Returns(nameof(settings.SubscriptionFileSharePath));
            A.CallTo(() => settings.SSRSUser).Returns(nameof(settings.SSRSUser));
            A.CallTo(() => settings.SSRSPassword).Returns(nameof(settings.SSRSPassword));
            A.CallTo(() => fileSystem.LoadXElement(string.Empty)).Returns(XElement.Parse(@"
<SubscriptionSettings>
  <ExtensionName>"+SubscriptionSettings.ReportServerFileShare+ @"</ExtensionName>
  <ExtensionParameters>
    <Parameter>
      <Name>PATH</Name>
      <Value>Sample</Value>
    </Parameter>
    <Parameter>
      <Name>USERNAME</Name>
      <Value>user</Value>
    </Parameter>
    <Parameter>
      <Name>PASSWORD</Name>
      <Value>password</Value>
    </Parameter>
  </ExtensionParameters>
</SubscriptionSettings>"));

            var instance = new SubscriptionSettings(log, settings, fileSystem);
            instance.Load(string.Empty);

            var extensionParameters = instance.ExtensionParameters;

            Assert.Equal(nameof(settings.SubscriptionFileSharePath), extensionParameters.Single(x => x.Name == "PATH").Value);
            Assert.Equal(nameof(settings.SSRSUser), extensionParameters.Single(x => x.Name == "USERNAME").Value);
            Assert.Equal(nameof(settings.SSRSPassword), extensionParameters.Single(x => x.Name == "PASSWORD").Value);
            A.CallTo(() => fileSystem.LoadXElement(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void LoadAndIgnoreNonFoundParameter()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            A.CallTo(() => settings.SubscriptionFileSharePath).Returns(nameof(settings.SubscriptionFileSharePath));
            A.CallTo(() => settings.SSRSUser).Returns(nameof(settings.SSRSUser));
            A.CallTo(() => settings.SSRSPassword).Returns(nameof(settings.SSRSPassword));
            A.CallTo(() => fileSystem.LoadXElement(string.Empty)).Returns(XElement.Parse(@"
<SubscriptionSettings>
  <ExtensionName>" + SubscriptionSettings.ReportServerFileShare + @"</ExtensionName>
  <ExtensionParameters>
    <Parameter>
      <Name>USERNAME</Name>
      <Value>user</Value>
    </Parameter>
    <Parameter>
      <Name>PASSWORD</Name>
      <Value>password</Value>
    </Parameter>
  </ExtensionParameters>
</SubscriptionSettings>"));

            var instance = new SubscriptionSettings(log, settings, fileSystem);
            instance.Load(string.Empty);

            var extensionParameters = instance.ExtensionParameters;

            Assert.True(extensionParameters.All(x => x.Name != "PATH"));
            Assert.Equal(nameof(settings.SSRSUser), extensionParameters.Single(x => x.Name == "USERNAME").Value);
            Assert.Equal(nameof(settings.SSRSPassword), extensionParameters.Single(x => x.Name == "PASSWORD").Value);
            A.CallTo(() => fileSystem.LoadXElement(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void Save()
        {
            var log = A.Fake<ILog>();
            var settings = A.Fake<ISettings>();
            var fileSystem = A.Fake<IFileSystemProxy>();

            var instance = new SubscriptionSettings(log, settings, fileSystem);

            instance.Save(string.Empty);

            A.CallTo(() => fileSystem.SaveXElement(A<XElement>._, A<string>._)).MustHaveHappened();
        }
    }
}
