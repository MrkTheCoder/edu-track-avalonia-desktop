namespace EduTrack.IntegrationTests.Fixtures
{
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestCollection 
        : ICollectionFixture<IntegrationTestFixture>
    {
        // This class has no code — its purpose is to link the fixture type
        // to the collection name.

        /*
            How its works:
             - All test classes marked [Collection("IntegrationTests")] will 
               share the same `IntegrationTestFixture` instance.
             - This avoids rebuilding our EF Core database for every single 
               test class.
         */
        
    }
}
