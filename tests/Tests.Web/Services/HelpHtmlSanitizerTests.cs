using Web.Services;

namespace Tests.Web.Services;

public class HelpHtmlSanitizerTests
{
    [Fact]
    public void GivenTargetBlankLink_WhenSanitize_ThenRemovesTarget()
    {
        // Arrange
        var sanitizer = new HelpHtmlSanitizer();

        // Act
        var sanitized = sanitizer.Sanitize("<a href=\"https://example.com\" target=\"_blank\">Lien</a>");

        // Assert
        sanitized.ShouldBe("<a href=\"https://example.com\">Lien</a>");
    }
}
