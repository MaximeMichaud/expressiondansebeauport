using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.ParticipantA)
            .WithMany()
            .HasForeignKey(c => c.ParticipantAMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ParticipantB)
            .WithMany()
            .HasForeignKey(c => c.ParticipantBMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.ParticipantAMemberId, c.ParticipantBMemberId }).IsUnique();

        builder.HasMany(c => c.Participants)
            .WithOne(cp => cp.Conversation)
            .HasForeignKey(cp => cp.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
