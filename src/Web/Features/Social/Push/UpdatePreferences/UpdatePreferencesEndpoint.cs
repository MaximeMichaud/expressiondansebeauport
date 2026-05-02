using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.UpdatePreferences;

public class UpdatePreferencesRequest
{
    public bool DirectMessage { get; set; }
    public bool Announcement { get; set; }
    public bool GroupPost { get; set; }
}

public class UpdatePreferencesEndpoint : Endpoint<UpdatePreferencesRequest, SucceededOrNotResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public UpdatePreferencesEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Put("social/push/preferences");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdatePreferencesRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        await _prefRepo.UpdatePreferences(user.Id, req.DirectMessage, req.Announcement, req.GroupPost);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
