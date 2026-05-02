using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.CreateSubscription;

public class CreateSubscriptionEndpoint : Endpoint<CreateSubscriptionRequest, SucceededOrNotResponse>
{
    private readonly IPushSubscriptionRepository _subRepo;
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IAuthenticatedUserService _authUser;

    public CreateSubscriptionEndpoint(
        IPushSubscriptionRepository subRepo,
        INotificationPreferencesRepository prefRepo,
        IAuthenticatedUserService authUser)
    {
        _subRepo = subRepo;
        _prefRepo = prefRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/push/subscriptions");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateSubscriptionRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;

        var existing = await _subRepo.FindByEndpoint(req.Endpoint);
        if (existing == null)
        {
            await _subRepo.Add(new PushSubscription(user.Id, req.Endpoint, req.P256dh, req.Auth));
        }
        else
        {
            existing.UpdateKeys(req.P256dh, req.Auth);
            existing.TouchLastUsed();
            await _subRepo.Update(existing);
        }

        await _prefRepo.GetOrCreate(user.Id);

        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
