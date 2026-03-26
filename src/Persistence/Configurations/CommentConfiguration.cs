using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Content).HasMaxLength(5000).IsRequired();

        builder.HasOne(c => c.AuthorMember)
            .WithMany()
            .HasForeignKey(c => c.AuthorMemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
