using System.Net;
using Application.Services.Push;
using Domain.Entities;
using Domain.Repositories;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using Tests.Application.TestCollections;
using PushSubscription = Domain.Entities.PushSubscription;

namespace Tests.Application.Services.Push;

[Collection(ApplicationTestCollection.NAME)]
public class PushNotificationDispatcherTests
{
    private readonly Mock<IPushSubscriptionRepository> _subRepo;
    private readonly Mock<INotificationPreferencesRepository> _prefRepo;
    private readonly Mock<IPushSenderClient> _client;
    private readonly Mock<ILogger<PushNotificationDispatcher>> _logger;
    private readonly PushNotificationDispatcher _dispatcher;

    private readonly Guid _userId = Guid.NewGuid();

    public PushNotificationDispatcherTests()
    {
        _subRepo = new Mock<IPushSubscriptionRepository>();
        _prefRepo = new Mock<INotificationPreferencesRepository>();
        _client = new Mock<IPushSenderClient>();
        _logger = new Mock<ILogger<PushNotificationDispatcher>>();

        _dispatcher = new PushNotificationDispatcher(_subRepo.Object, _prefRepo.Object, _client.Object, _logger.Object);
    }

    [Fact]
    public async Task GivenUserHasNoSubscriptions_WhenSendToUser_ThenDoesNothing()
    {
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(new UserNotificationPreferences(_userId));
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>());

        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage,
            new PushPayload { Title = "T", Body = "B" });

        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GivenUserHasDmDisabled_WhenSendDmToUser_ThenDoesNotSend()
    {
        var prefs = new UserNotificationPreferences(_userId);
        prefs.UpdatePreferences(dm: false, announcement: true, groupPost: true);
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>
        {
            new PushSubscription(_userId, "https://push.example/1", "p1", "a1")
        });

        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage,
            new PushPayload { Title = "T", Body = "B" });

        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GivenUserHasDmEnabled_WhenSendDmToUser_ThenSendsToAllSubscriptions()
    {
        var prefs = new UserNotificationPreferences(_userId);
        var sub1 = new PushSubscription(_userId, "https://push.example/1", "p1", "a1");
        var sub2 = new PushSubscription(_userId, "https://push.example/2", "p2", "a2");
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription> { sub1, sub2 });

        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage,
            new PushPayload { Title = "T", Body = "B" });

        _client.Verify(c => c.SendAsync(sub1, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _client.Verify(c => c.SendAsync(sub2, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenGroupPostAndUserHasMutedThatGroup_WhenSendGroupPost_ThenDoesNotSend()
    {
        var prefs = new UserNotificationPreferences(_userId);
        var groupId = Guid.NewGuid();
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _prefRepo.Setup(r => r.FindGroupOverride(_userId, groupId))
            .ReturnsAsync(new UserGroupNotificationPreferences(_userId, groupId, enabled: false));
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription>
        {
            new PushSubscription(_userId, "https://push.example/1", "p1", "a1")
        });

        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.GroupPost,
            new PushPayload { Title = "T", Body = "B", GroupId = groupId });

        _client.Verify(c => c.SendAsync(It.IsAny<PushSubscription>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GivenSendReturnsGone_WhenSendToUser_ThenDeletesExpiredSubscription()
    {
        var prefs = new UserNotificationPreferences(_userId);
        var sub = new PushSubscription(_userId, "https://push.example/1", "p1", "a1");
        _prefRepo.Setup(r => r.FindByUserId(_userId)).ReturnsAsync(prefs);
        _subRepo.Setup(r => r.GetByUserId(_userId)).ReturnsAsync(new List<PushSubscription> { sub });
        _client.Setup(c => c.SendAsync(sub, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PushServiceClientException("Gone", HttpStatusCode.Gone));

        await _dispatcher.SendToUserAsync(_userId, PushNotificationType.DirectMessage,
            new PushPayload { Title = "T", Body = "B" });

        _subRepo.Verify(r => r.DeleteByEndpoint(sub.Endpoint), Times.Once);
    }
}
