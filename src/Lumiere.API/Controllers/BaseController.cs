using Lumiere.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

public abstract class BaseController(ISender sender) : Controller
{
    protected readonly ISender _sender = sender;

    protected IActionResult Respond<T>(ResultDto<T> result)
    {
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }
}
