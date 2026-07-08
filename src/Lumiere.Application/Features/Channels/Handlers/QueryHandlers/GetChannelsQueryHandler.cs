using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Channels.Queries;
using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Lumiere.Application.Features.Channels.Handlers.QueryHandlers;

public class GetChannelsQueryHandler(IChannelRepository channelRepository)
    : IRequestHandler<GetChannelsQuery, ResultDto<BasePaginationResult<ChannelPaginated>>>
{
    public async Task<ResultDto<BasePaginationResult<ChannelPaginated>>> Handle(GetChannelsQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Channel, bool>> filterExpression = channel =>
            (string.IsNullOrEmpty(request.Name) || channel.Name.Contains(request.Name)) &&
            (string.IsNullOrEmpty(request.Description) || (channel.Description != null && channel.Description.Contains(request.Description))) &&
            (!request.Active.HasValue || channel.Active == request.Active) &&
            (!request.CreatedFrom.HasValue || channel.CreatedAt.Date >= request.CreatedFrom.Value.Date) &&
            (!request.CreatedTo.HasValue || channel.CreatedAt.Date <= request.CreatedTo.Value.Date);

        Expression<Func<Channel, object>> orderByExpression = channel => channel.Name;

        Expression<Func<Channel, ChannelPaginated>> selectorExpression = channel => new ChannelPaginated(
            channel.Name,
            channel.Active,
            channel.CreatedAt);

        PaginationFilters<Channel, ChannelPaginated> paginationFilter = new(
            request.Page!.Value,
            request.PageAmount!.Value,
            filterExpression,
            orderByExpression,
            selectorExpression);

        BasePaginationResult<ChannelPaginated> channelsPaginatedResult = await channelRepository
            .GetAllPaginationAsync(paginationFilter, cancellationToken);

        ResultDto<BasePaginationResult<ChannelPaginated>> result = new();
        result.SetData(channelsPaginatedResult);

        return result;
    }
}
