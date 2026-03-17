using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(pr => pr.Member)
            .WithMany()
            .HasForeignKey(pr => pr.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pr => new { pr.PostId, pr.MemberId }).IsUnique();
    }
}
