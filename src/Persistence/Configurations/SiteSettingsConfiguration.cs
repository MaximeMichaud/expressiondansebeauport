using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SiteTitle).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Tagline).HasMaxLength(200);
        builder.Property(s => s.PrimaryColor).HasMaxLength(7).IsRequired();
        builder.Property(s => s.SecondaryColor).HasMaxLength(7);
        builder.Property(s => s.HeadingFont).HasMaxLength(100).IsRequired();
        builder.Property(s => s.BodyFont).HasMaxLength(100).IsRequired();

        builder.HasOne(s => s.LogoMediaFile)
            .WithMany()
            .HasForeignKey(s => s.LogoMediaFileId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.FaviconMediaFile)
            .WithMany()
            .HasForeignKey(s => s.FaviconMediaFileId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
