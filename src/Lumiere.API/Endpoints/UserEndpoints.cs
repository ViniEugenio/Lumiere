using Lumiere.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Endpoints;

public class UserEndpoints : EndpointBase
{
    public static IEndpointRouteBuilder MapUserEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("users");

        group.MapPost("", CreateUser)
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> CreateUser(
        [FromServices] ISender sender,
        [FromBody] CreateUserCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return HandleResult(result);
    }
}
