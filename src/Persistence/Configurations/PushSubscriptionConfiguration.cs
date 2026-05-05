using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint).IsRequired().HasMaxLength(500);
        builder.Property(x => x.P256dh).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Auth).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.Endpoint).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
