using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.UpdateGroupPreference;

public class UpdateGroupPreferenceRequest
{
    public Guid GroupId { get; set; }
    public bool Enabled { get; set; }
}

public class UpdateGroupPreferenceEndpoint : Endpoint<UpdateGroupPreferenceRequest, SucceededOrNotResponse>
{
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public UpdateGroupPreferenceEndpoint(INotificationPreferencesRepository prefRepo, IAuthenticatedUserService authUser)
    {
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Put("social/push/preferences/groups/{GroupId}");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateGroupPreferenceRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        await _prefRepo.SetGroupOverride(user.Id, req.GroupId, req.Enabled);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
