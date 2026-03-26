using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PostViewConfiguration : IEntityTypeConfiguration<PostView>
{
    public void Configure(EntityTypeBuilder<PostView> builder)
    {
        builder.HasKey(pv => new { pv.PostId, pv.MemberId });

        builder.HasOne(pv => pv.Post)
            .WithMany()
            .HasForeignKey(pv => pv.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pv => pv.Member)
            .WithMany()
            .HasForeignKey(pv => pv.MemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
