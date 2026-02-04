using System.Text;
using Application.Services.Notifications.Dtos;
using AutoMapper;
using Infrastructure.Mailing.Mapping;
using SendGrid.Helpers.Mail;
using Tests.Common.Mapping;

namespace Tests.Infrastructure.Mailing;

public class MailingMappingProfileTests
{
    private const string AnyContentType = "image/png";
    private const string AnyFileName = "image.png";
    private const string AnyBodyStream = "eyuhwvo48wvnhfvebjntvw";
    
    private readonly IMapper _mapper;
    
    public MailingMappingProfileTests()
    {
        _mapper = new MapperBuilder().WithProfile<MailingMappingProfile>().Build();
    }
    
    [Fact]
    public void GivenAttachmentDtoWithAnyContentType_WhenMap_ThenReturnAttachementWithSameContentType()
    {
        // Arrange
        var attachementDto = new AttachmentDto
        {
            ContentType = AnyContentType
        };
        
        // Act
        var actual = _mapper.Map<Attachment>(attachementDto);

        // Assert
        actual.Type.ShouldBe(AnyContentType);
    }
    
    [Fact]
    public void GivenAttachmentDtoWithAnyFileName_WhenMap_ThenReturnAttachementWithSameFileName()
    {
        // Arrange
        var attachementDto = new AttachmentDto
        {
            FileName = AnyFileName
        };
        
        // Act
        var actual = _mapper.Map<Attachment>(attachementDto);

        // Assert
        actual.Filename.ShouldBe(AnyFileName);
    }
    
    [Fact]
    public void WhenMap_ThenReturnAttachementWithDispositionSetToAttachment()
    {
        // Arrange
        var attachementDto = new AttachmentDto();

        // Act
        var actual = _mapper.Map<Attachment>(attachementDto);

        // Assert
        actual.Disposition.ShouldBe("attachment");
    }
    
    [Fact]
    public void GivenAttachmentDtoWithAnyBodyStream_WhenMap_ThenReturnAttachementWithBase64Content()
    {
        // Arrange
        var attachementDto = new AttachmentDto
        {
            BodyStream = AnyBodyStream
        };

        // Act
        var actual = _mapper.Map<Attachment>(attachementDto);

        // Assert
        actual.Content.ShouldBe(Convert.ToBase64String(Encoding.UTF8.GetBytes(AnyBodyStream)));
    }
}