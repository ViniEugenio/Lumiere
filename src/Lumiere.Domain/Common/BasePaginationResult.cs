namespace Lumiere.Domain.Common;

public record BasePaginationResult<T>(
    int Page,
    int PageAmount,
    int TotalPages,
    List<T> Items);
