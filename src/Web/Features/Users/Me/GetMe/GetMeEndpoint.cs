using Application.Interfaces.Services.Users;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Users.Me.GetMe;

public class GetMeEndpoint : EndpointWithoutRequest<UserDto>
{
    private readonly IMapper _mapper;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public GetMeEndpoint(IMapper mapper, IAuthenticatedUserService authenticatedUserService)
    {
        _mapper = mapper;
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("users/me");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        var response = _mapper.Map<UserDto>(user);
        await Send.OkAsync(response, cancellation: ct);
    }
}