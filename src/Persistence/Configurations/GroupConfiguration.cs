using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.ImageUrl).HasMaxLength(500);
        builder.Property(g => g.InviteCode).HasMaxLength(50).IsRequired();
        builder.Property(g => g.Season).HasMaxLength(50).IsRequired();

        builder.HasIndex(g => g.InviteCode).IsUnique();

        builder.HasMany(g => g.Members)
            .WithOne(gm => gm.Group)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
