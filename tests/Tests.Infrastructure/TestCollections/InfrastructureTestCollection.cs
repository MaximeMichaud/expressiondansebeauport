using Tests.Infrastructure.Fixtures;

namespace Tests.Infrastructure.TestCollections;

[CollectionDefinition(NAME)]
public class InfrastructureTestCollection : ICollectionFixture<PersistenceTestFixture>
{
    // This class is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.

    public const string NAME = "Infrastructure Test Collection";
}