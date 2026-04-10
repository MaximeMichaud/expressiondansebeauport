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
        // SetParticipants(Member a, Member b) assigns based on Guid comparison order
        var a = CreateMember(participantAId);
        var b = CreateMember(participantBId);
        var c = new Conversation();
        c.SetId(id);
        c.SetParticipants(a, b);
        return c;
    }

    [Fact]
    public async Task GivenContentAndMedia_WhenSendMessage_ThenPersistsAllFields()
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

        var msg = await service.SendMessage(
            conversationId,
            senderId,
            "Salut!",
            "/uploads/social/foo.display.webp",
            "/uploads/social/foo.thumb.webp",
            "/uploads/social/foo.original.jpg");

        msg.Content.ShouldBe("Salut!");
        msg.MediaUrl.ShouldBe("/uploads/social/foo.display.webp");
        msg.MediaThumbnailUrl.ShouldBe("/uploads/social/foo.thumb.webp");
        msg.MediaOriginalUrl.ShouldBe("/uploads/social/foo.original.jpg");
    }

    [Fact]
    public async Task GivenMediaOnly_WhenSendMessage_ThenContentIsEmptyAndMediaPersisted()
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

        var msg = await service.SendMessage(
            conversationId,
            senderId,
            null,
            "/uploads/social/img.display.webp",
            "/uploads/social/img.thumb.webp",
            "/uploads/social/img.original.png");

        msg.Content.ShouldBe(string.Empty);
        msg.MediaUrl.ShouldBe("/uploads/social/img.display.webp");
    }

    [Fact]
    public async Task GivenContentOnly_WhenSendMessage_ThenMediaUrlsAreNull()
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

        var msg = await service.SendMessage(conversationId, senderId, "Hi", null, null, null);

        msg.Content.ShouldBe("Hi");
        msg.MediaUrl.ShouldBeNull();
        msg.MediaThumbnailUrl.ShouldBeNull();
        msg.MediaOriginalUrl.ShouldBeNull();
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
            () => service.SendMessage(conversationId, senderId, null, null, null, null));
    }
}
