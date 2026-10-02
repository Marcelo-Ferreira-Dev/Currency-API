using API.Application.Common;

namespace API.Endpoints;

internal static class EndpointResults
{
    public static IResult ToHttp<T>(this OperationResult<T> result, Func<T, IResult>? success = null)
    {
        if (result.Errors is not null)
        {
            return Results.ValidationProblem(result.Errors);
        }

        if (result.NotFound)
        {
            return Results.NotFound();
        }

        if (result.Conflict is not null)
        {
            return Results.Conflict(new
            {
                message = result.Conflict
            });
        }

        return success is null ? Results.Ok(result.Value) : success(result.Value!);
    }
}
