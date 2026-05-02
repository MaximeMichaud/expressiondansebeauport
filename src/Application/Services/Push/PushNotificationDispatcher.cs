using System.Net;
using System.Text.Json;
using Domain.Repositories;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using DomainPushSubscription = Domain.Entities.PushSubscription;
using DomainNotificationPreferences = Domain.Entities.UserNotificationPreferences;

namespace Application.Services.Push;

public class PushNotificationDispatcher : IPushNotificationDispatcher
{
    private readonly IPushSubscriptionRepository _subRepo;
    private readonly INotificationPreferencesRepository _prefRepo;
    private readonly IPushSenderClient _client;
    private readonly ILogger<PushNotificationDispatcher> _logger;

    public PushNotificationDispatcher(
        IPushSubscriptionRepository subRepo,
        INotificationPreferencesRepository prefRepo,
        IPushSenderClient client,
        ILogger<PushNotificationDispatcher> logger)
    {
        _subRepo = subRepo;
        _prefRepo = prefRepo;
        _client = client;
        _logger = logger;
    }

    public async Task SendToUserAsync(Guid userId, PushNotificationType type, PushPayload payload, CancellationToken ct = default)
    {
        var prefs = await _prefRepo.FindByUserId(userId);
        if (prefs == null) return;

        if (!IsTypeEnabled(prefs, type)) return;

        if (type == PushNotificationType.GroupPost && payload.GroupId.HasValue)
        {
            var groupOverride = await _prefRepo.FindGroupOverride(userId, payload.GroupId.Value);
            if (groupOverride != null && !groupOverride.Enabled) return;
        }

        var subs = await _subRepo.GetByUserId(userId);
        if (subs.Count == 0) return;

        var json = JsonSerializer.Serialize(new
        {
            title = payload.Title,
            body = payload.Body,
            url = payload.Url,
            tag = payload.Tag
        });

        var tasks = subs.Select(sub => SendOneAsync(sub, json, ct));
        await Task.WhenAll(tasks);
    }

    public async Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct = default)
    {
        var tasks = userIds.Select(uid => SendToUserAsync(uid, type, payload, ct));
        await Task.WhenAll(tasks);
    }

    private async Task SendOneAsync(DomainPushSubscription sub, string json, CancellationToken ct)
    {
        try
        {
            await _client.SendAsync(sub, json, ct);
        }
        catch (PushServiceClientException ex) when (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Push subscription {Endpoint} expired (status {Status}); deleting.", sub.Endpoint, ex.StatusCode);
            await _subRepo.DeleteByEndpoint(sub.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push send failed for subscription {Endpoint}", sub.Endpoint);
        }
    }

    private static bool IsTypeEnabled(DomainNotificationPreferences prefs, PushNotificationType type) => type switch
    {
        PushNotificationType.DirectMessage => prefs.NotifyOnDirectMessage,
        PushNotificationType.GroupPost => prefs.NotifyOnGroupPost,
        PushNotificationType.Announcement => prefs.NotifyOnAnnouncement,
        _ => false
    };
}
