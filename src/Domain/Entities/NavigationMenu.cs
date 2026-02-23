using Domain.Common;

namespace Domain.Entities;

public enum MenuLocation
{
    Primary,
    Footer
}

public class NavigationMenu : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public MenuLocation Location { get; private set; }
    public ICollection<NavigationMenuItem> MenuItems { get; private set; } = new List<NavigationMenuItem>();

    public NavigationMenu() { }

    public NavigationMenu(string name, MenuLocation location)
    {
        Name = name;
        Location = location;
    }

    public void SetName(string name) => Name = name;
    public void SetLocation(MenuLocation location) => Location = location;
}
