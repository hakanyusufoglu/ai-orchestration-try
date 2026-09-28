using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Errors;
using Shared.Results;

namespace Shared.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return result.Value is null ? Microsoft.AspNetCore.Http.Results.NoContent() : Microsoft.AspNetCore.Http.Results.Ok(result.Value);

        return result.Error!.Type switch
        {
            AppErrorType.NotFound     => Problem(result.Error, 404, "Not Found"),
            AppErrorType.Conflict     => Problem(result.Error, 409, "Conflict"),
            AppErrorType.Validation   => Problem(result.Error, 422, "Unprocessable Entity"),
            AppErrorType.Unauthorized => Problem(result.Error, 401, "Unauthorized"),
            _                         => Problem(result.Error, 500, "Internal Server Error")
        };
    }

    private static IResult Problem(AppError error, int status, string title) =>
        Microsoft.AspNetCore.Http.Results.Problem(new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = error.Message,
            Extensions = { ["errorCode"] = error.Code }
        });
}
