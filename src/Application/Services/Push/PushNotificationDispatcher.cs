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
        var subs = await ResolveSubscriptionsForUser(userId, type, payload);
        if (subs.Count == 0) return;

        var json = SerializePayload(payload);
        var tasks = subs.Select(sub => SendOneAsync(sub, json, ct));
        await Task.WhenAll(tasks);
    }

    public async Task SendToManyAsync(IEnumerable<Guid> userIds, PushNotificationType type, PushPayload payload, CancellationToken ct = default)
    {
        // Sequential DB access (avoids DbContext concurrency), parallel HTTP sends
        var allSubs = new List<DomainPushSubscription>();
        foreach (var uid in userIds)
        {
            allSubs.AddRange(await ResolveSubscriptionsForUser(uid, type, payload));
        }
        if (allSubs.Count == 0) return;

        var json = SerializePayload(payload);

        // Throttle parallel HTTP sends to avoid exhausting connection pool / push service rate limits
        using var throttle = new SemaphoreSlim(50);
        var tasks = allSubs.Select(async sub =>
        {
            await throttle.WaitAsync(ct);
            try { await SendOneAsync(sub, json, ct); }
            finally { throttle.Release(); }
        });
        await Task.WhenAll(tasks);
    }

    private async Task<List<DomainPushSubscription>> ResolveSubscriptionsForUser(Guid userId, PushNotificationType type, PushPayload payload)
    {
        var prefs = await _prefRepo.FindByUserId(userId);
        if (prefs == null) return new List<DomainPushSubscription>();

        if (!IsTypeEnabled(prefs, type)) return new List<DomainPushSubscription>();

        if (type == PushNotificationType.GroupPost && payload.GroupId.HasValue)
        {
            var groupOverride = await _prefRepo.FindGroupOverride(userId, payload.GroupId.Value);
            if (groupOverride != null && !groupOverride.Enabled) return new List<DomainPushSubscription>();
        }

        return await _subRepo.GetByUserId(userId);
    }

    private static string SerializePayload(PushPayload payload) => JsonSerializer.Serialize(new
    {
        title = payload.Title,
        body = payload.Body,
        url = payload.Url,
        tag = payload.Tag
    });

    private async Task SendOneAsync(DomainPushSubscription sub, string json, CancellationToken ct)
    {
        try
        {
            await _client.SendAsync(sub, json, ct);
        }
        catch (PushServiceClientException ex) when (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Push subscription expired (status {Status}); deleting.", ex.StatusCode);
            await _subRepo.DeleteByEndpoint(sub.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push send failed for one subscription");
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
