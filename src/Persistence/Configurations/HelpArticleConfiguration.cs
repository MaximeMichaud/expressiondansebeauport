using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class HelpArticleConfiguration : IEntityTypeConfiguration<HelpArticle>
{
    public void Configure(EntityTypeBuilder<HelpArticle> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.RouteHint);
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(200).IsRequired();
        builder.Property(p => p.RouteHint).HasMaxLength(200);
        builder.Property(p => p.ContentMode).HasMaxLength(10).HasDefaultValue("html");
        builder.Property(p => p.Category)
            .HasConversion<string>()
            .HasMaxLength(30);
    }
}
