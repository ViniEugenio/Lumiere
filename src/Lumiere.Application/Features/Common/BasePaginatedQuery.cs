using Lumiere.Application.DTOs;
using Lumiere.Domain.Common;
using MediatR;

namespace Lumiere.Application.Features.Common;

public record BasePaginatedQuery<TResult>(int? Page, int? PageAmount) : IRequest<ResultDto<BasePaginationResult<TResult>>>
{
    public int? Page { get; private set; } = Page ?? 1;
    public int? PageAmount { get; private set; } = PageAmount ?? 10;
}
