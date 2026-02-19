using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PageSectionConfiguration : IEntityTypeConfiguration<PageSection>
{
    public void Configure(EntityTypeBuilder<PageSection> builder)
    {
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.HtmlContent).HasColumnType("nvarchar(max)");
        builder.Property(s => s.ImageUrl).HasMaxLength(500);
    }
}
