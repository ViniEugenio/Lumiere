using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Channels.Queries;
using Lumiere.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

[Route("api/channel")]
public class ChannelController(ISender sender) : BaseController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetChannels([FromQuery] GetChannelsQuery query)
    {
        ResultDto<BasePaginationResult<ChannelPaginated>> result = await _sender.Send(query);
        return Respond(result);
    }
}
