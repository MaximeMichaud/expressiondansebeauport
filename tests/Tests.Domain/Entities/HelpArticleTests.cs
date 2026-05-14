using Domain.Entities;

namespace Tests.Domain.Entities;

public class HelpArticleTests
{
    [Fact]
    public void GivenFrenchTitle_WhenGenerateSlug_ThenRemovesAccentsAndUsesSeparators()
    {
        // Act
        var slug = HelpArticle.GenerateSlug("L'école de danse à Beauport!");

        // Assert
        slug.ShouldBe("l-ecole-de-danse-a-beauport");
    }

    [Fact]
    public void GivenWhitespaceRouteHint_WhenSetRouteHint_ThenStoresNull()
    {
        // Arrange
        var article = new HelpArticle("Titre", "titre", HelpCategory.Pages);

        // Act
        article.SetRouteHint("   ");

        // Assert
        article.RouteHint.ShouldBeNull();
    }

    [Fact]
    public void GivenPaddedRouteHint_WhenSetRouteHint_ThenTrimsValue()
    {
        // Arrange
        var article = new HelpArticle("Titre", "titre", HelpCategory.Pages);

        // Act
        article.SetRouteHint(" admin.children.pages.edit ");

        // Assert
        article.RouteHint.ShouldBe("admin.children.pages.edit");
    }
}
