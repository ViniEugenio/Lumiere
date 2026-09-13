using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers
{
    [Route("api/database")]
    public class DataBaseController(ISender sender) : BaseController(sender)
    {
    }
}
