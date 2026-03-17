using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.HasOne(cp => cp.Member)
            .WithMany()
            .HasForeignKey(cp => cp.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cp => new { cp.ConversationId, cp.MemberId }).IsUnique();
    }
}
