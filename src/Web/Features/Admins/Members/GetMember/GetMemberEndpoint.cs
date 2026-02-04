using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Members.GetMember;

public class GetMemberEndpoint : Endpoint<GetMemberRequest, MemberDto>
{
    private readonly IMapper _mapper;
    private readonly IMemberRepository _memberRepository;

    public GetMemberEndpoint(IMapper mapper, IMemberRepository memberRepository)
    {
        _mapper = mapper;
        _memberRepository = memberRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("members/{id}");

        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMemberRequest req, CancellationToken ct)
    {
        var member = _memberRepository.FindById(req.Id);
        var response = _mapper.Map<MemberDto>(member);
        await Send.OkAsync(response, cancellation: ct);
    }
}