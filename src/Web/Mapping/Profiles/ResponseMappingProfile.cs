using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Web.Dtos;
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

        CreateMap<Administrator, GetMeAdminResponse>();

        CreateMap<User, UserDto>()
            .ForMember(x => x.Roles, opt => opt.MapFrom(x => x.RoleNames))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.PhoneNumber))
            .ForMember(x => x.PhoneExtension, opt => opt.MapFrom(x => x.PhoneExtension));

        CreateMap<MediaFile, MediaFileDto>()
            .ForMember(x => x.FileType, opt => opt.MapFrom(x => x.FileType.ToString()));

        CreateMap<Page, PageDto>()
            .ForMember(x => x.Status, opt => opt.MapFrom(x => x.Status.ToString()))
            .ForMember(x => x.FeaturedImageUrl, opt => opt.MapFrom(x => x.FeaturedImage != null ? x.FeaturedImage.BlobUrl : null));

        CreateMap<NavigationMenu, NavigationMenuDto>()
            .ForMember(x => x.Location, opt => opt.MapFrom(x => x.Location.ToString()));

        CreateMap<NavigationMenuItem, NavigationMenuItemDto>()
            .ForMember(x => x.Target, opt => opt.MapFrom(x => x.Target.ToString()))
            .ForMember(x => x.PageSlug, opt => opt.MapFrom(x => x.Page != null ? x.Page.Slug : null));

        CreateMap<SiteSettings, SiteSettingsDto>()
            .ForMember(x => x.LogoUrl, opt => opt.MapFrom(x => x.LogoMediaFile != null ? x.LogoMediaFile.BlobUrl : null))
            .ForMember(x => x.FaviconUrl, opt => opt.MapFrom(x => x.FaviconMediaFile != null ? x.FaviconMediaFile.BlobUrl : null));
    }
}