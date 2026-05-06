namespace Web.Features.Common;

public class PaginateRequest
{
    public const int MaxPageSize = 100;

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int NormalizedPageIndex => PageIndex < 1 ? 1 : PageIndex;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 10,
        > MaxPageSize => MaxPageSize,
        _ => PageSize,
    };
}
