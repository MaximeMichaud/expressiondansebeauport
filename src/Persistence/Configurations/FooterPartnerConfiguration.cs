using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class FooterPartnerConfiguration : IEntityTypeConfiguration<FooterPartner>
{
    public void Configure(EntityTypeBuilder<FooterPartner> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.AltText).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Url).HasMaxLength(500);

        builder.HasOne(f => f.SiteSettings)
            .WithMany(ss => ss.FooterPartners)
            .HasForeignKey(f => f.SiteSettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.MediaFile)
            .WithMany()
            .HasForeignKey(f => f.MediaFileId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
