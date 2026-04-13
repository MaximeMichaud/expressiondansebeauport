namespace Application.Common;

public record PaginatedResult<T>(List<T> Items, bool HasMore);
