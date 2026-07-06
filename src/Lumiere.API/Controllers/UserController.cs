using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Queries;
using Lumiere.Domain.Common;
using Lumiere.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers
{
    [Route("user")]
    public class UserController(ISender sender) : Controller
    {

        private readonly ISender _sender = sender;

        [HttpGet]
        public async Task<IActionResult> GetUsersQuery([FromQuery] GetUsersQuery query)
        {
            
            ResultDto<BasePaginationResult<UserPaginated>> result = await _sender.Send(query);
            return Ok(result);

        }

    }
}
