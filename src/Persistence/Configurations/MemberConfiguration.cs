using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.LastName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.ProfileImageUrl).HasMaxLength(500);
        builder.Property(m => m.AvatarColor).HasMaxLength(20).HasDefaultValue("#1a1a1a");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.UserId).IsUnique();
    }
}
