using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Web.Features.Common;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Members.GetAllMembers;

public class SearchMembersEndpoint : Endpoint<PaginateRequest, PaginatedList<MemberDto>>
{
    private readonly IMapper _mapper;
    private readonly IMemberRepository _memberRepository;

    public SearchMembersEndpoint(IMapper mapper, IMemberRepository memberRepository)
    {
        _mapper = mapper;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("members");

        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(PaginateRequest req, CancellationToken ct)
    {
        var paginatedList = _memberRepository.GetAllPaginated(req.PageIndex, req.PageSize);
        var membersDto = _mapper.Map<List<MemberDto>>(paginatedList.Items);
        await Send.OkAsync(new PaginatedList<MemberDto>(membersDto, paginatedList.TotalItems), cancellation: ct);
    }
}