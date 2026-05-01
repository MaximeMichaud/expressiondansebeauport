using Domain.Entities;
using Web.Dtos;

namespace Web.Features.Public.Breadcrumbs;

public interface IBreadcrumbService
{
    IReadOnlyList<BreadcrumbDto> GetForPage(Page page);
}
