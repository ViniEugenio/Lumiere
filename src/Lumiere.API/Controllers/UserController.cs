using Lumiere.Application.Features.Commands.Users.CreateUser;
using Lumiere.Application.Features.Commands.Users.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

[Route("api/user")]
public class UserController(ISender sender) : BaseController(sender)
{

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserCommand command)
    {
        return await Respond(command);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        return await Respond(command);
    }

}
