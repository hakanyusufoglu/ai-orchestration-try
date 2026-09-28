using Shared.Errors;

namespace Shared.Results;

public sealed class Result<T>
{
    public T? Value { get; }
    public AppError? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value) { Value = value; IsSuccess = true; }
    private Result(AppError error) { Error = error; IsSuccess = false; }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(AppError error) => new(error);
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(AppError error) => Result<T>.Fail(error);
}
