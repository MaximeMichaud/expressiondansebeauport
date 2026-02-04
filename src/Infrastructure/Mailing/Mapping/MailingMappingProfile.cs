using System.Text;
using Application.Services.Notifications.Dtos;
using AutoMapper;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Mailing.Mapping;

public class MailingMappingProfile : Profile
{
    public MailingMappingProfile()
    {
        CreateMap<AttachmentDto, Attachment>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.ContentType))
            .ForMember(dest => dest.Filename, opt => opt.MapFrom(x => x.FileName))
            .ForMember(dest => dest.Disposition, opt => opt.MapFrom(x => "attachment"))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(x => Convert.ToBase64String(Encoding.UTF8.GetBytes(x.BodyStream))));
    }
}