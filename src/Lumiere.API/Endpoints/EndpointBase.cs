using Lumiere.Application.DTOs;

namespace Lumiere.API.Endpoints;

public abstract class EndpointBase
{
    protected static IResult HandleResult<T>(ResultDto<T> result)
    {
        if (result.Succeeded)
        {
            return Results.Ok(result.Data);
        }

        return Results.BadRequest(result.Errors);
    }
}
