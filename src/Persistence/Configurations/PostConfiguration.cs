using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Content).IsRequired();
        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(p => p.Group)
            .WithMany()
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.AuthorMember)
            .WithMany()
            .HasForeignKey(p => p.AuthorMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(p => p.Media)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reactions)
            .WithOne(r => r.Post)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Poll)
            .WithOne(pl => pl.Post)
            .HasForeignKey<Poll>(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
