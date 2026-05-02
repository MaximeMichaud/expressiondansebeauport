using Domain.Entities;

namespace Application.Services.Push;

public interface IPushSenderClient
{
    Task SendAsync(PushSubscription subscription, string payloadJson, CancellationToken ct);
}
