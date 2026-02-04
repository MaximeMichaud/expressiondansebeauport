using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Books;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Web.Dtos;
using Web.Features.Members.Books;
using GetMeMemberResponse = Web.Features.Members.Me.GetMe.GetMeResponse;
using GetMeAdminResponse = Web.Features.Admins.Me.GetMe.GetMeResponse;

namespace Web.Mapping.Profiles;

public class ResponseMappingProfile : Profile
{
    public ResponseMappingProfile()
    {
        CreateMap<IdentityResult, SucceededOrNotResponse>();

        CreateMap<IdentityError, Error>()
            .ForMember(error => error.ErrorType, opt => opt.MapFrom(identity => identity.Code))
            .ForMember(error => error.ErrorMessage, opt => opt.MapFrom(identity => identity.Description));

        CreateMap<Member, GetMeMemberResponse>()
            .ForMember(x => x.Roles, opt => opt.MapFrom(x => x.User.RoleNames))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.GetPhoneNumber()))
            .ForMember(x => x.PhoneExtension, opt => opt.MapFrom(x => x.GetPhoneExtension()));

        CreateMap<Administrator, GetMeAdminResponse>();

        CreateMap<Book, BookDto>()
            .ForMember(bookDto => bookDto.Created, opt => opt.MapFrom(book => book.Created.ToDateTimeUtc()))
            .ForMember(bookDto => bookDto.NameFr, opt => opt.MapFrom(book => book.NameFr))
            .ForMember(bookDto => bookDto.NameEn, opt => opt.MapFrom(book => book.NameEn))
            .ForMember(bookDto => bookDto.DescriptionFr, opt => opt.MapFrom(book => book.DescriptionFr))
            .ForMember(bookDto => bookDto.DescriptionEn, opt => opt.MapFrom(book => book.DescriptionEn));

        CreateMap<User, UserDto>()
            .ForMember(x => x.Roles, opt => opt.MapFrom(x => x.RoleNames))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber))
            .ForMember(x => x.PhoneExtension, opt => opt.MapFrom(x => x.PhoneExtension));

        CreateMap<Member, MemberDto>()
            .ForMember(x => x.Roles, opt => opt.MapFrom(x => x.User.RoleNames))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber!.Number))
            .ForMember(x => x.PhoneExtension, opt => opt.MapFrom(x => x.PhoneNumber!.Extension));
    }
}