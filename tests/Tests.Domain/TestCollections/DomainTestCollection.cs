using Tests.Common.Fixtures;

namespace Tests.Domain.TestCollections;

[CollectionDefinition(NAME)]
public class DomainTestCollection : ICollectionFixture<TestFixture>
{
    // This class is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
    
    public const string NAME = "Domain Test Collection";
}