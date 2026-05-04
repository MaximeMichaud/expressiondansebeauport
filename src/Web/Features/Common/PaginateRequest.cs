namespace Web.Features.Common;

public class PaginateRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int NormalizedPageIndex => PageIndex < 1 ? 1 : PageIndex;

    public int NormalizedPageSize => PageSize < 1 ? 10 : PageSize;
}
