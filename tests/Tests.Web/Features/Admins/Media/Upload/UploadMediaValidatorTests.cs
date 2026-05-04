using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Media.Upload;

namespace Tests.Web.Features.Admins.Media.Upload;

public class UploadMediaValidatorTests
{
    [Fact]
    public void GivenAllowedImage_WhenValidate_ThenIsValid()
    {
        // Arrange
        var request = new UploadMediaRequest
        {
            File = CreateFile("photo.jpg", "image/jpeg", 1024)
        };

        // Act
        var result = new UploadMediaValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GivenHtmlFile_WhenValidate_ThenInvalidFileType()
    {
        // Arrange
        var request = new UploadMediaRequest
        {
            File = CreateFile("poc.html", "text/html", 1024)
        };

        // Act
        var result = new UploadMediaValidator().Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorCode == "InvalidFileType");
    }

    [Fact]
    public void GivenTooLargeFile_WhenValidate_ThenFileTooLarge()
    {
        // Arrange
        var request = new UploadMediaRequest
        {
            File = CreateFile("video.mp4", "video/mp4", 50 * 1024 * 1024 + 1)
        };

        // Act
        var result = new UploadMediaValidator().Validate(request);

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
