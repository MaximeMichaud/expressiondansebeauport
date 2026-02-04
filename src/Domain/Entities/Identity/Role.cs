using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class Role : IdentityRole<Guid>
{
    public List<UserRole> UserRoles { get; set; } = default!;
}