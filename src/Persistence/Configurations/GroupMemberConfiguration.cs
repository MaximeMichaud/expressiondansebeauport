using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.HasKey(gm => gm.Id);
        builder.Property(gm => gm.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(gm => gm.Member)
            .WithMany()
            .HasForeignKey(gm => gm.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(gm => new { gm.GroupId, gm.MemberId }).IsUnique();
    }
}
