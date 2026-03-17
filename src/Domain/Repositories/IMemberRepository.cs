using Domain.Entities;

namespace Domain.Repositories;

public interface IMemberRepository
{
    Task Add(Member member);
    Member? FindByUserId(Guid userId, bool asNoTracking = true);
    Member? FindById(Guid id, bool asNoTracking = true);
    Task<List<Member>> Search(string? search, int skip, int take);
    Task<int> Count(string? search);
    Task Update(Member member);
}
