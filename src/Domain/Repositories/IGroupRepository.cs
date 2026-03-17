using Domain.Entities;

namespace Domain.Repositories;

public interface IGroupRepository
{
    Task Add(Group group);
    Task<Group?> FindById(Guid id, bool asNoTracking = true);
    Task<Group?> FindByInviteCode(string code);
    Task<List<Group>> GetBySeason(string season, bool includeArchived = false);
    Task<List<Group>> GetActive();
    Task<List<Group>> GetAll();
    Task Update(Group group);
    Task<List<string>> GetDistinctSeasons();
}
