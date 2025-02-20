using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace FileMutator.IntegrationTest
{
    public class JobServiceFixtureAspire : IAsyncLifetime
    {
        private const string JobServiceName = "apiservice";
        private DistributedApplication app = null!;
        public Uri? BaseAddress;
        public async Task InitializeAsync()
        {
            var args = new[] { "--test" };
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FileMutator_Web>(args);

            app = await appHost.BuildAsync();
            await app.StartAsync();

            BaseAddress = app.GetEndpoint(JobServiceName);
        }

        public async Task DisposeAsync()
        {
            await this.app.DisposeAsync();
        }
    }

    [CollectionDefinition("JobServiceFixtureAspire")]
    public class NotUsedInCode : ICollectionFixture<JobServiceFixtureAspire>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

}
