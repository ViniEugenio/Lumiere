using Lumiere.Application.DTOs.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

public abstract class BaseController(ISender sender) : Controller
{
    protected readonly ISender _sender = sender;

    protected async Task<IActionResult> Respond(IRequest<ResultDto> request)
    {

        ResultDto response = await _sender.Send(request);

        if(!response.IsValid())
        {
            return BadRequest(response);
        }

        return Ok(response);

    }
}
