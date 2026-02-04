using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IMemberRepository
{
    PaginatedList<Member> GetAllPaginated(int pageIndex, int pageSize);
    Member FindById(Guid id);
    Member? FindByUserId(Guid userId, bool asNoTracking = true);
    Member? FindByUserEmail(string userEmail);
    Task Create(Member member);
    Task Update(Member member);
}