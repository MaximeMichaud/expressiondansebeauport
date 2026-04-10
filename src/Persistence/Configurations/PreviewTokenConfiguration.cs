using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PreviewTokenConfiguration : IEntityTypeConfiguration<PreviewToken>
{
    public void Configure(EntityTypeBuilder<PreviewToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Page)
            .WithMany()
            .HasForeignKey(t => t.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.Property(t => t.Token).HasMaxLength(64).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(256);
    }
}
