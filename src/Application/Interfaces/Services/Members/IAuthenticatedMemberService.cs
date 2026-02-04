using Domain.Entities;

namespace Application.Interfaces.Services.Members;

public interface IAuthenticatedMemberService
{
    Member GetAuthenticatedMember();
}