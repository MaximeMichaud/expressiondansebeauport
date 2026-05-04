using System.Reflection;
using Persistence;

namespace Tests.Infrastructure.Persistence;

public class GarneauTemplateDbContextInitializerTests
{
    [Fact]
    public void GivenDuplicatedUploadsPath_WhenRewriteSeedMediaUrls_ThenReturnSingleUploadsPath()
    {
        // Arrange
        const string fileName = "image-devant-studio.jpg";
        const string content = """
                               {"url":"/uploads/uploads/image-devant-studio.jpg"}
                               """;

        // Act
        var result = RewriteSeedMediaUrls(content, [fileName]);

        // Assert
        result.ShouldBe("""
                        {"url":"/uploads/image-devant-studio.jpg"}
                        """);
    }

    private static string RewriteSeedMediaUrls(string content, IEnumerable<string> fileNames)
    {
        var method = typeof(GarneauTemplateDbContextInitializer).GetMethod(
            "RewriteSeedMediaUrls",
            BindingFlags.NonPublic | BindingFlags.Static);

        method.ShouldNotBeNull();

        var result = method.Invoke(null, [content, fileNames]);
        return result.ShouldBeOfType<string>();
    }
}
