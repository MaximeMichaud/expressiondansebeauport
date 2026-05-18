using Web.Features.Admins.HelpArticles.Create;

namespace Tests.Web.Features.Admins.HelpArticles.Create;

public class CreateHelpArticleValidatorTests
{
    [Fact]
    public void GivenValidRequest_WhenValidate_ThenIsValid()
    {
        // Arrange
        var request = new CreateHelpArticleRequest
        {
            Title = "Modifier une page",
            Slug = "modifier-une-page",
            Category = "Pages",
            ContentMode = "html",
        };

        // Act
        var result = new CreateHelpArticleValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenTooLongSlug_WhenValidate_ThenSlugTooLong()
    {
        // Arrange
        var request = new CreateHelpArticleRequest
        {
            Title = "Modifier une page",
            Slug = new string('a', 201),
            Category = "Pages",
            ContentMode = "html",
        };

        // Act
        var result = new CreateHelpArticleValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "SlugTooLong");
    }
}
