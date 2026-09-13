using Lumiere.Application.Features.Users.CreateUser;
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

}
