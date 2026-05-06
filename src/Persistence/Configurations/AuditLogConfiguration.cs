using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserDisplayName).HasMaxLength(200);
        builder.Property(x => x.UserEmail).HasMaxLength(256);
        builder.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(4000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ActionType);
    }
}
