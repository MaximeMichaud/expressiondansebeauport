using Domain.Common;

namespace Domain.Entities;

public enum MenuItemTarget
{
    Self,
    Blank
}

public class NavigationMenuItem : Entity, IComparable<NavigationMenuItem>
{
    public Guid MenuId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Label { get; private set; } = null!;
    public string? Url { get; private set; }
    public Guid? PageId { get; private set; }
    public int SortOrder { get; private set; }
    public MenuItemTarget Target { get; private set; } = MenuItemTarget.Self;

    public NavigationMenu Menu { get; private set; } = null!;
    public NavigationMenuItem? Parent { get; private set; }
    public ICollection<NavigationMenuItem> Children { get; private set; } = new List<NavigationMenuItem>();
    public Page? Page { get; private set; }

    public NavigationMenuItem() { }

    public NavigationMenuItem(Guid menuId, string label, int sortOrder)
    {
        MenuId = menuId;
        Label = label;
        SortOrder = sortOrder;
    }

    public void SetLabel(string label) => Label = label;
    public void SetUrl(string? url) => Url = url;
    public void SetPageId(Guid? pageId) => PageId = pageId;
    public void SetParentId(Guid? parentId) => ParentId = parentId;
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
    public void SetTarget(MenuItemTarget target) => Target = target;

    public int CompareTo(NavigationMenuItem? other) => SortOrder.CompareTo(other?.SortOrder ?? 0);
}
