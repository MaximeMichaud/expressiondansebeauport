using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class NavigationMenuConfiguration : IEntityTypeConfiguration<NavigationMenu>
{
    public void Configure(EntityTypeBuilder<NavigationMenu> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Location)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(m => m.MenuItems)
            .WithOne(i => i.Menu)
            .HasForeignKey(i => i.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
