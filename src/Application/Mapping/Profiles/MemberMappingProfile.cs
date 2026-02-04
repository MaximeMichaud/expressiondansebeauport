using Application.Services.Members.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Identity;

namespace Application.Mapping.Profiles;

// ReSharper disable once ClassNeverInstantiated.Global
public class MemberMappingProfile : Profile
{
    public MemberMappingProfile()
    {
        CreateMap<Member, Member>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .IgnoreAuditableEntityProperties();

        CreateMap<Member, User>()
            .ForMember(user => user.Id, opt => opt.Ignore())
            .ForMember(user => user.Email, opt => opt.MapFrom(member => member.Email.ToLower()))
            .ForMember(user => user.UserName, opt => opt.MapFrom(member => member.Email.ToLower()))
            .ForMember(user => user.NormalizedEmail, opt => opt.MapFrom(member => member.Email.Normalize()))
            .ForMember(user => user.NormalizedUserName, opt => opt.MapFrom(member => member.Email.Normalize()))
            .ForMember(user => user.PhoneNumber, opt => opt.MapFrom(member => member.GetPhoneNumber()))
            .ForMember(user => user.PhoneExtension, opt => opt.MapFrom(member => member.GetPhoneExtension()))
            .ForMember(user => user.EmailConfirmed, opt => opt.MapFrom(_ => true))
            .ForMember(user => user.PhoneNumberConfirmed, opt => opt.MapFrom(_ => true))
            .ForMember(user => user.TwoFactorEnabled, opt => opt.MapFrom(_ => true));

        CreateMap<MemberRegistrationDto, Member>()
            .ConstructUsing(dto => new Member(
                dto.FirstName,
                dto.LastName,
                dto.Apartment,
                dto.Street,
                dto.City,
                dto.ZipCode));
    }
}