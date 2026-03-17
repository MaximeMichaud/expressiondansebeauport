using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PollVoteConfiguration : IEntityTypeConfiguration<PollVote>
{
    public void Configure(EntityTypeBuilder<PollVote> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.HasOne(pv => pv.Member)
            .WithMany()
            .HasForeignKey(pv => pv.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pv => new { pv.PollOptionId, pv.MemberId }).IsUnique();
    }
}
