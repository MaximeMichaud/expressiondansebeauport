using Domain.Common;
using Domain.Enums;
using NodaTime;

namespace Domain.Entities;

public class PostReaction : Entity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public ReactionType Type { get; private set; }
    public Instant CreatedAt { get; private set; }

    public void SetPost(Post post)
    {
        Post = post;
        PostId = post.Id;
    }

    public void SetMember(Member member)
    {
        Member = member;
        MemberId = member.Id;
    }

    public void SetType(ReactionType type) => Type = type;
    public void SetCreatedAt(Instant createdAt) => CreatedAt = createdAt;
}
