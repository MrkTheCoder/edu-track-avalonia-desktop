using EduTrack.IntegrationTests.TestHelpers;

namespace EduTrack.IntegrationTests.Fixtures
{
    /*
         This fixture sets up shared resources for our integration tests — 
        such as a database context, web server, or test data.
     */
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public IntegrationTestBase Base { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Base = new IntegrationTestBase();
            await Base.ResetDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            await Base.DisposeAsync();
        }
    }
}
