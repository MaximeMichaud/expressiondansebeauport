using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Members.UpdateMe;

public class UpdateMeEndpoint : Endpoint<UpdateMeRequest, SucceededOrNotResponse>
{
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;
    private readonly IUserRepository _userRepository;

    public UpdateMeEndpoint(
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository,
        IUserRepository userRepository)
    {
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
        _userRepository = userRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("social/members/me");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMeRequest req, CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        var member = _memberRepository.FindByUserId(user.Id, asNoTracking: false);
        if (member == null) { await Send.NotFoundAsync(ct); return; }

        member.SetFirstName(req.FirstName.Trim());
        member.SetLastName(req.LastName.Trim());
        member.SanitizeForSaving();

        // Update email on the User entity
        var newEmail = req.Email.Trim().ToLowerInvariant();
        if (newEmail != user.Email)
        {
            var existingUser = _userRepository.FindByEmail(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                var error = new Error("EmailExists", "Un compte avec ce courriel existe déjà.");
                await Send.OkAsync(new SucceededOrNotResponse(false, error), ct);
                return;
            }

            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = newEmail.ToUpperInvariant();
            user.NormalizedUserName = newEmail.ToUpperInvariant();
        }

        await _memberRepository.Update(member);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
