using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class UserRole : IdentityUserRole<Guid>
{
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}