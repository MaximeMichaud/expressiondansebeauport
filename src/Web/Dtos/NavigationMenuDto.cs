namespace Web.Dtos;

public class NavigationMenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public List<NavigationMenuItemDto> MenuItems { get; set; } = new();
}

public class NavigationMenuItemDto
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public Guid? PageId { get; set; }
    public string? PageSlug { get; set; }
    public int SortOrder { get; set; }
    public string Target { get; set; } = "Self";
    public List<NavigationMenuItemDto> Children { get; set; } = new();
}
