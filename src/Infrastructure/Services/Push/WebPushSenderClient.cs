using Application.Services.Push;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LibPushSubscription = Lib.Net.Http.WebPush.PushSubscription;

namespace Infrastructure.Services.Push;

public class WebPushSenderClient : IPushSenderClient
{
    private readonly PushServiceClient? _client;
    private readonly ILogger<WebPushSenderClient> _logger;

    public WebPushSenderClient(IConfiguration config, ILogger<WebPushSenderClient> logger)
    {
        _logger = logger;

        var subject = config["Vapid:Subject"];
        var publicKey = config["Vapid:PublicKey"];
        var privateKey = config["Vapid:PrivateKey"];

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey))
        {
            _logger.LogWarning("Web push notifications are disabled because VAPID configuration is incomplete.");
            return;
        }

        try
        {
            var vapid = new VapidAuthentication(publicKey, privateKey) { Subject = subject };
            _client = new PushServiceClient { DefaultAuthentication = vapid };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Web push notifications are disabled because VAPID configuration is invalid.");
        }
    }

    public Task SendAsync(Domain.Entities.PushSubscription subscription, string payloadJson, CancellationToken ct)
    {
        if (_client is null)
            return Task.CompletedTask;

        var libSub = new LibPushSubscription { Endpoint = subscription.Endpoint };
        libSub.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
        libSub.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

        var msg = new PushMessage(payloadJson);
        return _client.RequestPushMessageDeliveryAsync(libSub, msg, ct);
    }
}
