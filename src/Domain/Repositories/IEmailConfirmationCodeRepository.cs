using Domain.Entities;

namespace Domain.Repositories;

public interface IEmailConfirmationCodeRepository
{
    Task Add(EmailConfirmationCode code);
    Task<EmailConfirmationCode?> GetLatestForUser(Guid userId);
    Task Update(EmailConfirmationCode code);
}
