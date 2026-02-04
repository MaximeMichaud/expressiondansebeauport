using Tests.Common.Fixtures;

namespace Tests.Application.TestCollections;

[CollectionDefinition(NAME)]
public class ApplicationTestCollection : ICollectionFixture<TestFixture>
{
    // This class is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
    
    public const string NAME = "Application Test Collection";
}