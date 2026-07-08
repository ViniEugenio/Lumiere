using FluentValidation;
using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Channels.Queries;

namespace Lumiere.Application.Validators;

public class GetChannelsQueryValidator : AbstractValidator<GetChannelsQuery>
{
    public GetChannelsQueryValidator()
    {
        Include(new BasePaginationValidator<ChannelPaginated>());
    }
}
