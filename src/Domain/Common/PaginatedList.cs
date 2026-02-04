namespace Domain.Common;

public class PaginatedList<T>(IEnumerable<T> items, int totalItems)
{
    public int TotalItems { get; set; } = totalItems;
    public IEnumerable<T> Items { get; set; } = items;
}