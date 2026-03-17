using Domain.Entities;

namespace Application.Services.Members;

public interface IMemberRegistrationService
{
    Task<(Member member, string confirmationCode)> RegisterMember(
        string firstName, string lastName, string email, string password);
}
