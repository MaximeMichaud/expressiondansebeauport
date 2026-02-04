using Domain.Entities.Identity;
using Tests.Common.Builders;

namespace Tests.Common.Fixtures;

public class TestFixture
{
    public UserBuilder UserBuilder;
    public MemberBuilder MemberBuilder;
    public AdministratorBuilder AdministratorBuilder;

    protected Guid AnyRoleId;

    private readonly Random _random;

    public TestFixture()
    {
        _random = new Random();
        UserBuilder = new UserBuilder();
        MemberBuilder = new MemberBuilder();
        AdministratorBuilder = new AdministratorBuilder();
    }

    public Role BuildRole(string roleName)
    {
        return new Role { Name = roleName, NormalizedName = roleName.ToUpperInvariant() };
    }

    public int GenerateCrmId()
    {
        return _random.Next(200000000, 300000000);
    }

    public string GenerateEmail()
    {
        return $"john.doe.{GenerateCrmId()}@gmail.com";
    }

    public void ResetBuilders()
    {
        UserBuilder = new UserBuilder();
        MemberBuilder = new MemberBuilder();
        AdministratorBuilder = new AdministratorBuilder();
    }
}