using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class NavigationMenuItemConfiguration : IEntityTypeConfiguration<NavigationMenuItem>
{
    public void Configure(EntityTypeBuilder<NavigationMenuItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Label).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Url).HasMaxLength(500);
        builder.Property(i => i.Target)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasOne(i => i.Parent)
            .WithMany(i => i.Children)
            .HasForeignKey(i => i.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(i => i.Page)
            .WithMany()
            .HasForeignKey(i => i.PageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
