using Application.Services.Notifications.Dtos;

namespace Tests.Application.Services.Notifications.Dtos;

public class AttachmentDtoTests
{
    private const string AnyFileName = "file-name";
    private const string AnyFileStream = "file-stream";
    private const string AnyFileContentType = "file-content-type";
    
    [Fact]
    public void GivenAnyContentType_WhenNewAttachmentDto_ThenContentTypeShouldBeSameAsGivenContentType()
    {
        // Act
        var attachmentDto = new AttachmentDto { ContentType = AnyFileContentType };

        // Assert
        attachmentDto.ContentType.ShouldBe(AnyFileContentType);
    }
    
    [Fact]
    public void GivenAnyFileName_WhenNewAttachmentDto_ThenFileNameShouldBeSameAsGivenFileName()
    {
        // Act
        var attachmentDto = new AttachmentDto { FileName = AnyFileName };

        // Assert
        attachmentDto.FileName.ShouldBe(AnyFileName);
    }
    
    [Fact]
    public void GivenAnyFileStream_WhenNewAttachmentDto_ThenBodyStreamShouldBeSameAsGivenBodyStream()
    {
        // Act
        var attachmentDto = new AttachmentDto { BodyStream = AnyFileStream };

        // Assert
        attachmentDto.BodyStream.ShouldBe(AnyFileStream);
    }
}