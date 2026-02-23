using Domain.Repositories;
using FastEndpoints;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Public.SiteSettings;

public class GetPublicSiteSettingsEndpoint : EndpointWithoutRequest<SiteSettingsDto>
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public GetPublicSiteSettingsEndpoint(ISiteSettingsRepository settingsRepository, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/site-settings");
        AllowAnonymous();
        Options(x => x.CacheOutput(p => p.Expire(TimeSpan.FromHours(1))));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = await _settingsRepository.Get();
        await Send.OkAsync(_mapper.Map<SiteSettingsDto>(settings), cancellation: ct);
    }
}
