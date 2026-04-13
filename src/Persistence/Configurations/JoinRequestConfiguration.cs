using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
    public void Configure(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.HasKey(jr => jr.Id);

        builder.Property(jr => jr.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(jr => jr.Group)
            .WithMany()
            .HasForeignKey(jr => jr.GroupId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(jr => jr.RequesterMember)
            .WithMany()
            .HasForeignKey(jr => jr.RequesterMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(jr => jr.ResolvedByMember)
            .WithMany()
            .HasForeignKey(jr => jr.ResolvedByMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(jr => new { jr.GroupId, jr.RequesterMemberId, jr.Status });
    }
}
