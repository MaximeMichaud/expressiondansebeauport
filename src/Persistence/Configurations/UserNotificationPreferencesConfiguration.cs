using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class UserNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreferences> builder)
    {
        builder.ToTable("UserNotificationPreferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.NotifyOnDirectMessage).HasDefaultValue(true);
        builder.Property(x => x.NotifyOnAnnouncement).HasDefaultValue(true);
        builder.Property(x => x.NotifyOnGroupPost).HasDefaultValue(true);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
