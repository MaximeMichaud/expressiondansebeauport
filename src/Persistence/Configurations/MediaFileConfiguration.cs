using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.FileName).IsUnique();
        builder.Property(m => m.FileName).HasMaxLength(500).IsRequired();
        builder.Property(m => m.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(m => m.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(m => m.BlobUrl).HasMaxLength(2000).IsRequired();
        builder.Property(m => m.AltText).HasMaxLength(200);
    }
}
