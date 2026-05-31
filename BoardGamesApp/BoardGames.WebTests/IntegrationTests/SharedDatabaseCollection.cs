using Xunit;

namespace BoardGames.WebTests.IntegrationTests
{
    [CollectionDefinition("SharedDatabase")]
    public class SharedDatabaseCollection : ICollectionFixture<SharedDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
