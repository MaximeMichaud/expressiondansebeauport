namespace Web.Features.Common;

public class PaginateRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}