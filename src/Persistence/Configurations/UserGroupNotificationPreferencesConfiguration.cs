using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class UserGroupNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserGroupNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<UserGroupNotificationPreferences> builder)
    {
        builder.ToTable("UserGroupNotificationPreferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.GroupId }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
