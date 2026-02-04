using Application.Interfaces.Services;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Members.DeleteMember;

public class DeleteMemberEndpoint : Endpoint<DeleteMemberRequest, EmptyResponse>
{
    private readonly IMemberRepository _memberRepository;
    private readonly IHttpContextUserService _httpContextUserService;

    public DeleteMemberEndpoint(
        IMemberRepository memberRepository,
        IHttpContextUserService httpContextUserService)
    {
        _memberRepository = memberRepository;
        _httpContextUserService = httpContextUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Delete("members/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMemberRequest req, CancellationToken ct)
    {
        var member = _memberRepository.FindById(req.Id);
        member.SoftDelete(_httpContextUserService.Username);
        await _memberRepository.Update(member);
        await Send.NoContentAsync(ct);
    }
}