using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Content).HasMaxLength(5000).IsRequired();
        builder.Property(m => m.MediaUrl).HasMaxLength(500);
        builder.Property(m => m.MediaThumbnailUrl).HasMaxLength(500);
        builder.Property(m => m.MediaOriginalUrl).HasMaxLength(500);

        builder.HasOne(m => m.SenderMember)
            .WithMany()
            .HasForeignKey(m => m.SenderMemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
