using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.GetAllPages;

public class GetAllPagesEndpoint : EndpointWithoutRequest<List<PageDto>>
{
    private readonly IMapper _mapper;
    private readonly IPageRepository _pageRepository;

    public GetAllPagesEndpoint(IMapper mapper, IPageRepository pageRepository)
    {
        _mapper = mapper;
        _pageRepository = pageRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/pages");

        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pages = _pageRepository.GetAll();
        var response = _mapper.Map<List<PageDto>>(pages);
        await Send.OkAsync(response, cancellation: ct);
    }
}
