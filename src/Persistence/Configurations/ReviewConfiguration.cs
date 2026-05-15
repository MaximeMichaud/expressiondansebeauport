using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(600).IsRequired();
        builder.Property(r => r.Author).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Rating).IsRequired();

        builder.HasOne(r => r.SiteSettings)
            .WithMany(ss => ss.Reviews)
            .HasForeignKey(r => r.SiteSettingsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
