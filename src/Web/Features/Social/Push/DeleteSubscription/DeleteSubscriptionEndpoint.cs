using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Push.DeleteSubscription;

public class DeleteSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
}

public class DeleteSubscriptionEndpoint : Endpoint<DeleteSubscriptionRequest, SucceededOrNotResponse>
{
    private readonly IPushSubscriptionRepository _subRepo;
    private readonly IAuthenticatedUserService _authUser;

    public DeleteSubscriptionEndpoint(IPushSubscriptionRepository subRepo, IAuthenticatedUserService authUser)
    {
        _subRepo = subRepo;
        _authUser = authUser;
    }

    public override void Configure()
    {
        Delete("social/push/subscriptions");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteSubscriptionRequest req, CancellationToken ct)
    {
        var user = _authUser.GetAuthenticatedUser()!;
        var existing = await _subRepo.FindByEndpoint(req.Endpoint);
        if (existing != null && existing.UserId == user.Id)
        {
            await _subRepo.DeleteByEndpoint(req.Endpoint);
        }
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
