using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.Get;

public class GetSiteSettingsEndpoint : EndpointWithoutRequest<SiteSettingsDto>
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public GetSiteSettingsEndpoint(ISiteSettingsRepository settingsRepository, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/site-settings");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = _settingsRepository.Get();
        await Send.OkAsync(_mapper.Map<SiteSettingsDto>(settings), cancellation: ct);
    }
}
