using Lumiere.Application.Features.Database.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers
{
    [Route("api/database")]
    public class DataBaseController(ISender sender) : BaseController(sender)
    {
        [HttpPost]
        public async Task<IActionResult> UpdateDataBase([FromBody] UpdateDataBaseCommand command)
        {
            var result = await _sender.Send(command);
            return Respond(result);
        }
    }
}
