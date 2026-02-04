using Application.Interfaces.Services.Members;
using Application.Services.Members.Dtos;
using Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Members.CreateMember;

public class CreateMemberEndpoint : EndpointWithSanitizedRequest<CreateMemberRequest, SucceededOrNotResponse>
{
    private readonly IMapper _mapper;
    private readonly IMemberRegistrationService _memberRegistrationService;

    public CreateMemberEndpoint(
        IMapper mapper,
        IMemberRegistrationService memberRegistrationService)
    {
        _mapper = mapper;
        _memberRegistrationService = memberRegistrationService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Post("members");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateMemberRequest req, CancellationToken ct)
    {
        var memberRegistrationDto = _mapper.Map<MemberRegistrationDto>(req);
        await _memberRegistrationService.RegisterMember(memberRegistrationDto);
        await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
    }
}