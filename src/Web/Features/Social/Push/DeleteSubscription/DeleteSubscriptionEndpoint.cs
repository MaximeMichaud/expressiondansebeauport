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

    public DeleteSubscriptionEndpoint(IPushSubscriptionRepository subRepo) => _subRepo = subRepo;

    public override void Configure()
    {
        Delete("social/push/subscriptions");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteSubscriptionRequest req, CancellationToken ct)
    {
        await _subRepo.DeleteByEndpoint(req.Endpoint);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}
