using Application.Services.Members.Dtos;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Identity;
using Web.Dtos;
using Web.Features.Admins.Members.CreateMember;
using Web.Features.Admins.Members.UpdateMember;

namespace Web.Mapping.Profiles;

public class RequestMappingProfile : Profile
{
    public RequestMappingProfile()
    {
        CreateMap<TranslatableStringDto, TranslatableString>().ReverseMap();

        CreateMap<CreateMemberRequest, MemberRegistrationDto>();

        CreateMap<UpdateMemberRequest, User>()
            .ForMember(user => user.Id, opt => opt.Ignore())
            .ForMember(user => user.Email, opt => opt.MapFrom(request => request.Email.ToLower()))
            .ForMember(user => user.UserName, opt => opt.MapFrom(request => request.Email.ToLower()))
            .ForMember(user => user.NormalizedEmail, opt => opt.MapFrom(request => request.Email.ToUpper()))
            .ForMember(user => user.NormalizedUserName, opt => opt.MapFrom(request => request.Email.ToUpper()));
        CreateMap<UpdateMemberRequest, Member>();
    }
}