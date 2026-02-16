using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(200);

        builder.HasMany(p => p.Sections)
            .WithOne(s => s.Page)
            .HasForeignKey(s => s.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
