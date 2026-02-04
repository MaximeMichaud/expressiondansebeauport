using Application.Services.Members.Dtos;
using Application.Services.Users.Dtos;
using AutoMapper;
using Domain.Constants.User;
using Domain.Entities.Identity;

namespace Application.Mapping.Profiles;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, User>()
            .ForMember(user => user.UserRoles, opt => opt.Ignore());

        CreateMap<MemberRegistrationDto, UserCreationDto>()
            .ForMember(x => x.RoleName, opt => opt.MapFrom(x => Roles.MEMBER));

        CreateMap<UserCreationDto, User>()
            .ForMember(user => user.Id, opt => opt.Ignore())
            .ForMember(user => user.Email, opt => opt.MapFrom(dto => dto.Email.ToLower()))
            .ForMember(user => user.UserName, opt => opt.MapFrom(dto => dto.Email.ToLower()))
            .ForMember(user => user.NormalizedEmail, opt => opt.MapFrom(dto => dto.Email.ToUpper()))
            .ForMember(user => user.NormalizedUserName, opt => opt.MapFrom(dto => dto.Email.ToUpper()))
            .ForMember(user => user.PhoneNumber, opt => opt.MapFrom(dto => dto.PhoneNumber))
            .ForMember(user => user.PhoneExtension, opt => opt.MapFrom(dto => dto.PhoneExtension))
            .ForMember(user => user.EmailConfirmed, opt => opt.MapFrom(_ => true))
            .ForMember(user => user.PhoneNumberConfirmed, opt => opt.MapFrom(_ => true))
            .ForMember(user => user.TwoFactorEnabled, opt => opt.MapFrom(_ => true));
    }
}