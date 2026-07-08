using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;
using Lumiere.Application.Features.Users.Queries;
using Lumiere.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

[Route("api/user")]
public class UserController(ISender sender) : BaseController(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        ResultDto<BasePaginationResult<UserPaginated>> result = await _sender.Send(query);
        return Respond(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _sender.Send(command);
        return Respond(result);
    }
}
