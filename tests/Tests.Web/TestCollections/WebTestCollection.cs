using Tests.Common.Fixtures;

namespace Tests.Web.TestCollections;

[CollectionDefinition(NAME)]
public class WebTestCollection : ICollectionFixture<TestFixture>
{
    // This class is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
    
    public const string NAME = "Web Test Collection";
}