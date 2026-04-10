using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class MessageMediaConfiguration : IEntityTypeConfiguration<MessageMedia>
{
    public void Configure(EntityTypeBuilder<MessageMedia> builder)
    {
        builder.HasKey(mm => mm.Id);
        builder.Property(mm => mm.MediaUrl).HasMaxLength(500).IsRequired();
        builder.Property(mm => mm.ThumbnailUrl).HasMaxLength(500);
        builder.Property(mm => mm.OriginalUrl).HasMaxLength(500);
        builder.Property(mm => mm.ContentType).HasMaxLength(100).IsRequired();

        builder.HasOne(mm => mm.Message)
            .WithMany(m => m.Media)
            .HasForeignKey(mm => mm.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
