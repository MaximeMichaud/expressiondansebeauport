using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PageRevisionConfiguration : IEntityTypeConfiguration<PageRevision>
{
    public void Configure(EntityTypeBuilder<PageRevision> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Page)
            .WithMany()
            .HasForeignKey(r => r.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.PageId, r.RevisionNumber });
        builder.HasIndex(r => new { r.PageId, r.RevisionType });

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.MetaDescription).HasMaxLength(320);
        builder.Property(r => r.ContentMode).HasMaxLength(10).HasDefaultValue("html");
        builder.Property(r => r.RevisionType)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
    }
}
