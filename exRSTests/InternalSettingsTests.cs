using exRS;
using Xunit;

namespace SSRSManagerTests
{
    public class InternalSettingsTests
    {
        [Theory]
        [InlineData(nameof(ISettings.SSRSUser))]
        [InlineData(nameof(ISettings.SSRSPassword))]
        [InlineData(nameof(ISettings.BackupPath))]
        [InlineData(nameof(ISettings.SubscriptionSettingsFolder))]
        [InlineData(nameof(ISettings.DBUserForDataSource))]
        [InlineData(nameof(ISettings.DBPasswordForDataSource))]
        [InlineData(nameof(ISettings.SourceFolderPath))]
        [InlineData(nameof(ISettings.DataSourcesServerFolderName))]
        [InlineData(nameof(ISettings.DatasetsServerFolderName))]
        [InlineData(nameof(ISettings.SubscriptionFileSharePath))]
        public void CheckProperties(string property)
        {
            var settings = new ConfigurationSettings();

            var value = settings.GetType().GetProperty(property).GetValue(settings);

            Assert.Equal(property, value);
        }

        [Fact]
        public void CheckSSRSUri()
        {
            var settings = new ConfigurationSettings();

            var value = settings.SsrsUri;

            Assert.Equal("http:\\\\ssrsUri", value);
        }
    }
}
