using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Platform).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Url).HasMaxLength(500).IsRequired();

        builder.HasOne(s => s.SiteSettings)
            .WithMany(ss => ss.SocialLinks)
            .HasForeignKey(s => s.SiteSettingsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
