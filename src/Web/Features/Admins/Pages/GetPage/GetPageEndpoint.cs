using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.GetPage;

public class GetPageEndpoint : Endpoint<GetPageRequest, PageDto>
{
    private readonly IMapper _mapper;
    private readonly IPageRepository _pageRepository;

    public GetPageEndpoint(IMapper mapper, IPageRepository pageRepository)
    {
        _mapper = mapper;
        _pageRepository = pageRepository;
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
        var response = _mapper.Map<PageDto>(page);
        await Send.OkAsync(response, cancellation: ct);
    }
}
