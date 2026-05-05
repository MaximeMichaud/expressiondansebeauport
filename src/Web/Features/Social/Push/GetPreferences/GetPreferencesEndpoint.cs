using Application.Interfaces.Services.Users;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.GetPreferences;

public record MutedGroupDto(Guid GroupId);
public record GetPreferencesResponse(
    bool DirectMessage,
    bool Announcement,
    bool GroupPost,
    List<MutedGroupDto> MutedGroups
);

public class GetPreferencesEndpoint : EndpointWithoutRequest<GetPreferencesResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public GetPreferencesEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Get("social/push/preferences");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        var prefs = await _prefRepo.GetOrCreate(user.Id);
        var overrides = await _prefRepo.GetGroupOverridesForUser(user.Id);
        var muted = overrides.Where(o => !o.Enabled).Select(o => new MutedGroupDto(o.GroupId)).ToList();

        await Send.OkAsync(new GetPreferencesResponse(
            prefs.NotifyOnDirectMessage,
            prefs.NotifyOnAnnouncement,
            prefs.NotifyOnGroupPost,
            muted
        ), ct);
    }
}
