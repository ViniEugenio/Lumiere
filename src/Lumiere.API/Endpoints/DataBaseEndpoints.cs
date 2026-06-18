using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Database.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lumiere.API.Endpoints
{
    public class DataBaseEndpoints : EndpointBase
    {

        public static IEndpointRouteBuilder MapDataBaseEndpoints(IEndpointRouteBuilder endpoints)
        {

            RouteGroupBuilder routeGroup = endpoints.MapGroup("/database");

            routeGroup
                .MapPost("update-database", ApplyMigrations)
                .WithTags("Database")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            return endpoints;

        }

        private static async Task<IResult> ApplyMigrations([FromServices] ISender sender, CancellationToken cancellationToken)
        {

            ResultDto<object> result = await sender.Send(new UpdateDataBaseCommand(), cancellationToken);
            return HandleResult(result);

        }

    }
}
