using Domain.Common;

namespace Domain.Entities;

public class Comment : AuditableAndSoftDeletableEntity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; } = null!;
    public Guid AuthorMemberId { get; private set; }
    public Member AuthorMember { get; private set; } = null!;
    public string Content { get; private set; } = null!;

    public void SetPost(Post post)
    {
        Post = post;
        PostId = post.Id;
    }

    public void SetAuthor(Member member)
    {
        AuthorMember = member;
        AuthorMemberId = member.Id;
    }

    public void SetContent(string content) => Content = content;
}
