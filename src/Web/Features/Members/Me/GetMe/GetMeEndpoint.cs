using Application.Interfaces.Services.Members;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Members.Me.GetMe;

public class GetMeEndpoint : EndpointWithoutRequest<GetMeResponse>
{
    private readonly IMapper _mapper;
    private readonly IAuthenticatedMemberService _authenticatedMemberService;

    public GetMeEndpoint(IMapper mapper, IAuthenticatedMemberService authenticatedMemberService)
    {
        _mapper = mapper;
        _authenticatedMemberService = authenticatedMemberService;
    }

    public override void Configure()
    {
        Get("members/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var member = _authenticatedMemberService.GetAuthenticatedMember();
        var response = _mapper.Map<GetMeResponse>(member);
        await Send.OkAsync(response, cancellation: ct);
    }
}