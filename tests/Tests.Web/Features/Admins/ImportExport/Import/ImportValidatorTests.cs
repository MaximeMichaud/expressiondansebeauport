using Microsoft.AspNetCore.Http;
using Web.Features.Admins.ImportExport.Import;

namespace Tests.Web.Features.Admins.ImportExport.Import;

public class ImportValidatorTests
{
    [Fact]
    public void GivenJsonFile_WhenValidate_ThenIsValid()
    {
        // Arrange
        var request = new ImportRequest
        {
            File = CreateFile("export.json", "application/json", 1024)
        };

        // Act
        var result = new ImportValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenNonJsonFile_WhenValidate_ThenInvalidFileType()
    {
        // Arrange
        var request = new ImportRequest
        {
            File = CreateFile("export.txt", "text/plain", 1024)
        };

        // Act
        var result = new ImportValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "InvalidFileType");
    }

    [Fact]
    public void GivenTooLargeJsonFile_WhenValidate_ThenFileTooLarge()
    {
        // Arrange
        var request = new ImportRequest
        {
            File = CreateFile("export.json", "application/json", 10 * 1024 * 1024 + 1)
        };

        // Act
        var result = new ImportValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "FileTooLarge");
    }

    private static IFormFile CreateFile(string fileName, string contentType, long length)
    {
        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns(fileName);
        file.Setup(x => x.ContentType).Returns(contentType);
        file.Setup(x => x.Length).Returns(length);
        return file.Object;
    }
}
