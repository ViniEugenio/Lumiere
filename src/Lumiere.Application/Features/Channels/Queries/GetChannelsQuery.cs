using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Common;

namespace Lumiere.Application.Features.Channels.Queries;

public record GetChannelsQuery(
    string? Name,
    string? Description,
    bool? Active,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    int? Page,
    int? PageAmount) : BasePaginatedQuery<ChannelPaginated>(Page, PageAmount);
