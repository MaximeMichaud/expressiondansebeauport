using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.MediaUrl).HasMaxLength(500).IsRequired();
        builder.Property(pm => pm.ThumbnailUrl).HasMaxLength(500);
        builder.Property(pm => pm.ContentType).HasMaxLength(100).IsRequired();
    }
}
