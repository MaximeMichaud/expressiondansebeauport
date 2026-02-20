using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Web.Features.Common;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.GetAll;

public class GetAllPagesRequest : PaginateRequest
{
    public string? Status { get; set; }
}

public class GetAllPagesEndpoint : Endpoint<GetAllPagesRequest, PaginatedList<PageDto>>
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public GetAllPagesEndpoint(IPageRepository pageRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/pages");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAllPagesRequest req, CancellationToken ct)
    {
        PageStatus? statusFilter = null;
        if (Enum.TryParse<PageStatus>(req.Status, true, out var parsed))
            statusFilter = parsed;

        var paginatedList = _pageRepository.GetAllPaginated(req.PageIndex, req.PageSize, statusFilter);
        var dtos = _mapper.Map<List<PageDto>>(paginatedList.Items);
        await Send.OkAsync(new PaginatedList<PageDto>(dtos, paginatedList.TotalItems), cancellation: ct);
    }
}
