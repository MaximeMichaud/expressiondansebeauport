using Domain.Common;

namespace Domain.Entities;

public class Group : AuditableAndSoftDeletableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public string InviteCode { get; private set; } = null!;
    public string Season { get; private set; } = null!;
    public bool IsArchived { get; private set; }

    public ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();

    public void SetName(string name) => Name = name;
    public void SetDescription(string? description) => Description = description;
    public void SetImageUrl(string? url) => ImageUrl = url;
    public void SetInviteCode(string code) => InviteCode = code;
    public void SetSeason(string season) => Season = season;
    public void Archive() => IsArchived = true;
    public void Unarchive() => IsArchived = false;
}
