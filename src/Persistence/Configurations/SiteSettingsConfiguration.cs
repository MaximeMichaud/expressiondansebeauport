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
        builder.Property(s => s.FooterDescription).HasMaxLength(500);
        builder.Property(s => s.FooterAddress).HasMaxLength(200);
        builder.Property(s => s.FooterCity).HasMaxLength(100);
        builder.Property(s => s.FooterPhone).HasMaxLength(20);
        builder.Property(s => s.FooterEmail).HasMaxLength(100);
        builder.Property(s => s.FacebookUrl).HasMaxLength(500);
        builder.Property(s => s.InstagramUrl).HasMaxLength(500);
        builder.Property(s => s.CopyrightText).HasMaxLength(200);
        builder.Property(s => s.IsMaintenanceMode).HasDefaultValue(false).IsRequired();
        builder.Property(s => s.MaintenanceMessage).HasMaxLength(500).HasDefaultValue("Le site est en maintenance. Revenez bientôt !");
        builder.Property(s => s.MaintenanceRetryAfter).HasDefaultValue(3600).IsRequired();

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
