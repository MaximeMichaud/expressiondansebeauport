using NodaTime;

namespace Domain.Entities;

public class PostView
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public Member Member { get; private set; } = null!;
    public Instant ViewedAt { get; private set; }

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

    public void SetViewedAt(Instant viewedAt) => ViewedAt = viewedAt;
}
