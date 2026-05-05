namespace Application.Services.Push;

public interface IPushNotificationDispatcher
{
    Task SendToUserAsync(Guid userId, PushNotificationType type, PushPayload payload, CancellationToken ct = default);
    Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct = default);
}
