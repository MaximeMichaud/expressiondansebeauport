using Application.Interfaces.Services.Admins;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Me.GetMe;

public class GetMeEndpoint : EndpointWithoutRequest<GetMeResponse>
{
    private readonly IMapper _mapper;
    private readonly IAuthenticatedAdminService _authenticatedAdminService;

    public GetMeEndpoint(IMapper mapper, IAuthenticatedAdminService authenticatedAdminService)
    {
        _mapper = mapper;
        _authenticatedAdminService = authenticatedAdminService;
    }

    public override void Configure()
    {
        Get("admins/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var admin = _authenticatedAdminService.GetAuthenticatedAdmin();
        var response = _mapper.Map<GetMeResponse>(admin);
        await Send.OkAsync(response, cancellation: ct);
    }
}