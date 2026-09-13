using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Controllers;

[Route("api/channel")]
public class ChannelController(ISender sender) : BaseController(sender)
{
}
