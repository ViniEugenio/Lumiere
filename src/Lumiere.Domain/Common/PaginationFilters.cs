using System.Linq.Expressions;

namespace Lumiere.Domain.Common;

public record PaginationFilters<T, TResult>(
    int Page,
    int PageAmount,
    Expression<Func<T, bool>> FilterExpression,
    Expression<Func<T, object>> OrderByExpression,
    Expression<Func<T, TResult>> SelectorExpression);
