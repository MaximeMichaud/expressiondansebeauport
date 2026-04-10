using Application.Services.Messaging;
using Domain.Entities;
using Domain.Repositories;

namespace Tests.Application.Services.Messaging;

public class ConversationServiceSendMessageMediaTests
{
    private readonly Mock<IConversationRepository> _conversationRepository = new();
    private readonly Mock<IMessageRepository> _messageRepository = new();
    private readonly Mock<IMemberRepository> _memberRepository = new();

    private ConversationService CreateService() => new(
        _conversationRepository.Object,
        _messageRepository.Object,
        _memberRepository.Object);

    private static Member CreateMember(Guid id)
    {
        var m = new Member();
        m.SetId(id);
        return m;
    }

    private static Conversation CreateConversation(Guid id, Guid participantAId, Guid participantBId)
    {
        var a = CreateMember(participantAId);
        var b = CreateMember(participantBId);
        var c = new Conversation();
        c.SetId(id);
        c.SetParticipants(a, b);
        return c;
    }

    private static MessageMediaItem MakeMedia(int n) => new MessageMediaItem
    {
        DisplayUrl = $"/uploads/social/{n}.display.webp",
        ThumbnailUrl = $"/uploads/social/{n}.thumb.webp",
        OriginalUrl = $"/uploads/social/{n}.original.jpg",
        ContentType = "image/jpeg",
        Size = 12345
    };

    [Fact]
    public async Task GivenContentAnd3Media_WhenSendMessage_ThenPersistsOrderedMessageMedia()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var conversation = CreateConversation(conversationId, senderId, otherId);
        var sender = CreateMember(senderId);

        _conversationRepository.Setup(r => r.FindById(conversationId, false)).ReturnsAsync(conversation);
        _memberRepository.Setup(r => r.FindById(senderId, false)).Returns(sender);
        _messageRepository.Setup(r => r.Add(It.IsAny<Message>())).Returns(Task.CompletedTask);
        _messageRepository.Setup(r => r.MarkAsRead(conversationId, senderId)).Returns(Task.CompletedTask);

        var service = CreateService();
        var media = new[] { MakeMedia(1), MakeMedia(2), MakeMedia(3) };

        var msg = await service.SendMessage(conversationId, senderId, "Salut!", media);

        msg.Content.ShouldBe("Salut!");
        msg.Media.Count.ShouldBe(3);
        msg.Media.ElementAt(0).MediaUrl.ShouldBe("/uploads/social/1.display.webp");
        msg.Media.ElementAt(0).ThumbnailUrl.ShouldBe("/uploads/social/1.thumb.webp");
        msg.Media.ElementAt(0).OriginalUrl.ShouldBe("/uploads/social/1.original.jpg");
        msg.Media.ElementAt(0).SortOrder.ShouldBe(0);
        msg.Media.ElementAt(2).SortOrder.ShouldBe(2);
    }

    [Fact]
    public async Task GivenMediaOnly_WhenSendMessage_ThenContentIsEmpty()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var conversation = CreateConversation(conversationId, senderId, otherId);
        var sender = CreateMember(senderId);

        _conversationRepository.Setup(r => r.FindById(conversationId, false)).ReturnsAsync(conversation);
        _memberRepository.Setup(r => r.FindById(senderId, false)).Returns(sender);
        _messageRepository.Setup(r => r.Add(It.IsAny<Message>())).Returns(Task.CompletedTask);

        var service = CreateService();

        var msg = await service.SendMessage(conversationId, senderId, null, new[] { MakeMedia(1) });

        msg.Content.ShouldBe(string.Empty);
        msg.Media.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GivenContentOnly_WhenSendMessage_ThenMediaIsEmpty()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var conversation = CreateConversation(conversationId, senderId, otherId);
        var sender = CreateMember(senderId);

        _conversationRepository.Setup(r => r.FindById(conversationId, false)).ReturnsAsync(conversation);
        _memberRepository.Setup(r => r.FindById(senderId, false)).Returns(sender);
        _messageRepository.Setup(r => r.Add(It.IsAny<Message>())).Returns(Task.CompletedTask);

        var service = CreateService();

        var msg = await service.SendMessage(conversationId, senderId, "Hi", Array.Empty<MessageMediaItem>());

        msg.Content.ShouldBe("Hi");
        msg.Media.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GivenNoContentAndNoMedia_WhenSendMessage_ThenThrows()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var conversation = CreateConversation(conversationId, senderId, otherId);
        var sender = CreateMember(senderId);

        _conversationRepository.Setup(r => r.FindById(conversationId, false)).ReturnsAsync(conversation);
        _memberRepository.Setup(r => r.FindById(senderId, false)).Returns(sender);

        var service = CreateService();

        await Should.ThrowAsync<InvalidOperationException>(
            () => service.SendMessage(conversationId, senderId, null, Array.Empty<MessageMediaItem>()));
    }

    [Fact]
    public async Task GivenMoreThanTenMedia_WhenSendMessage_ThenThrows()
    {
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var conversation = CreateConversation(conversationId, senderId, otherId);
        var sender = CreateMember(senderId);

        _conversationRepository.Setup(r => r.FindById(conversationId, false)).ReturnsAsync(conversation);
        _memberRepository.Setup(r => r.FindById(senderId, false)).Returns(sender);

        var service = CreateService();
        var tooMany = Enumerable.Range(0, 11).Select(MakeMedia).ToArray();

        await Should.ThrowAsync<InvalidOperationException>(
            () => service.SendMessage(conversationId, senderId, "x", tooMany));
    }
}
