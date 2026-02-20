using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Get;

public class GetPageRequest
{
    public Guid Id { get; set; }
}

public class GetPageEndpoint : Endpoint<GetPageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public GetPageEndpoint(IPageRepository pageRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/pages/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetPageRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.Id);
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(_mapper.Map<PageDto>(page), cancellation: ct);
    }
}
