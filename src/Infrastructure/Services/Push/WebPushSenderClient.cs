using Application.Services.Push;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using LibPushSubscription = Lib.Net.Http.WebPush.PushSubscription;

namespace Infrastructure.Services.Push;

public class WebPushSenderClient : IPushSenderClient
{
    private readonly PushServiceClient _client;

    public WebPushSenderClient(IConfiguration config)
    {
        var subject = config["Vapid:Subject"] ?? throw new InvalidOperationException("Vapid:Subject missing");
        var publicKey = config["Vapid:PublicKey"] ?? throw new InvalidOperationException("Vapid:PublicKey missing");
        var privateKey = config["Vapid:PrivateKey"] ?? throw new InvalidOperationException("Vapid:PrivateKey missing");

        var vapid = new VapidAuthentication(publicKey, privateKey) { Subject = subject };
        _client = new PushServiceClient { DefaultAuthentication = vapid };
    }

    public Task SendAsync(Domain.Entities.PushSubscription subscription, string payloadJson, CancellationToken ct)
    {
        var libSub = new LibPushSubscription { Endpoint = subscription.Endpoint };
        libSub.SetKey(PushEncryptionKeyName.P256DH, subscription.P256dh);
        libSub.SetKey(PushEncryptionKeyName.Auth, subscription.Auth);

        var msg = new PushMessage(payloadJson);
        return _client.RequestPushMessageDeliveryAsync(libSub, msg, ct);
    }
}
