using Common.Logging;
using DryIoc;
using exRS;
using FakeItEasy;
using System;
using Xunit;

namespace SSRSManagerTests
{
    public class ModuleTests
    {
        [Fact]
        public void CreateModule()
        {
            var container = A.Fake<IContainer>();

            container = Module.Install(container);

            A.CallTo(container).Where(x => x.Method.Name == nameof(IContainer.Register))
                .MustHaveHappened(Repeated.Exactly.Times(9));
        }

        [Fact]
        public void GetClient()
        {
            var settings = A.Fake<ISettings>();

            var client = Module.ReportServiceFactory.Create(settings);

            Assert.NotNull(client);
        }
    }
}
