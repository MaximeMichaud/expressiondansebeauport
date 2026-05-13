using Web.Features.Admins.HelpArticles.Update;

namespace Tests.Web.Features.Admins.HelpArticles.Update;

public class UpdateHelpArticleValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenBlankSlug_WhenValidate_ThenSlugRequired(string? slug)
    {
        // Arrange
        var request = new UpdateHelpArticleRequest
        {
            Id = Guid.NewGuid(),
            Title = "Modifier une page",
            Slug = slug,
            Category = "Pages",
            ContentMode = "html",
        };

        // Act
        var result = new UpdateHelpArticleValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "SlugRequired");
    }

    [Fact]
    public void GivenTooLongSlug_WhenValidate_ThenSlugTooLong()
    {
        // Arrange
        var request = new UpdateHelpArticleRequest
        {
            Id = Guid.NewGuid(),
            Title = "Modifier une page",
            Slug = new string('a', 201),
            Category = "Pages",
            ContentMode = "html",
        };

        // Act
        var result = new UpdateHelpArticleValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "SlugTooLong");
    }
}
