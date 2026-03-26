using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class EmailConfirmationCodeConfiguration : IEntityTypeConfiguration<EmailConfirmationCode>
{
    public void Configure(EntityTypeBuilder<EmailConfirmationCode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(6).IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.IsUsed });
    }
}
