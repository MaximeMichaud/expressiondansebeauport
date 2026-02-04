using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => new {ur.UserId, ur.RoleId});
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired(false);
        
        builder.HasOne(ur => ur.User)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
    }
}