using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Post : AuditableAndSoftDeletableEntity
{
    public Guid? GroupId { get; private set; }
    public Group? Group { get; private set; }
    public Guid AuthorMemberId { get; private set; }
    public Member AuthorMember { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public PostType Type { get; private set; }
    public bool IsPinned { get; private set; }
    public int ViewCount { get; private set; }

    public ICollection<PostMedia> Media { get; private set; } = new List<PostMedia>();
    public ICollection<PostReaction> Reactions { get; private set; } = new List<PostReaction>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public Poll? Poll { get; private set; }

    public void SetGroup(Group? group)
    {
        Group = group;
        GroupId = group?.Id;
    }

    public void SetGroupId(Guid? groupId) => GroupId = groupId;

    public void SetAuthor(Member member)
    {
        AuthorMember = member;
        AuthorMemberId = member.Id;
    }

    public void SetContent(string content) => Content = content;
    public void SetType(PostType type) => Type = type;
    public void Pin() => IsPinned = true;
    public void Unpin() => IsPinned = false;
    public void IncrementViewCount() => ViewCount++;

    public bool IsAnnouncement => GroupId == null;
}
