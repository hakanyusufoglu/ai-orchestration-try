namespace Shared.Errors;

public sealed record AppError(string Code, string Message, AppErrorType Type = AppErrorType.Failure)
{
    public static AppError NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"'{entity}' with id '{id}' was not found.", AppErrorType.NotFound);

    public static AppError Conflict(string message) =>
        new("Conflict", message, AppErrorType.Conflict);

    public static AppError Validation(string message) =>
        new("Validation", message, AppErrorType.Validation);

    public static AppError Unauthorized() =>
        new("Unauthorized", "You are not authorized.", AppErrorType.Unauthorized);

    public static AppError ConcurrencyConflict() =>
        new("ConcurrencyConflict", "The record was modified by another request. Please retry.", AppErrorType.Conflict);
}
