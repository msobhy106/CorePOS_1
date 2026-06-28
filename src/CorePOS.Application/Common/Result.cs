namespace CorePOS.Application.Common;

/// <summary>
/// Unified result wrapper for all CQRS responses.
/// Eliminates exceptions for expected business errors.
/// </summary>
public class Result<T>
{
    public bool    IsSuccess { get; }
    public bool    IsFailure => !IsSuccess;
    public T?      Value     { get; }
    public string  Error     { get; } = string.Empty;
    public int     ErrorCode { get; }   // HTTP-like: 400,404,409,422...

    protected Result(bool isSuccess, T? value, string error, int errorCode = 0)
    {
        IsSuccess = isSuccess;
        Value     = value;
        Error     = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value)
        => new(true, value, string.Empty, 0);

    public static Result<T> Failure(string error, int errorCode = 400)
        => new(false, default, error, errorCode);

    public static Result<T> NotFound(string error = "Record not found")
        => new(false, default, error, 404);

    public static Result<T> Conflict(string error)
        => new(false, default, error, 409);

    public static Result<T> Unauthorized(string error = "Unauthorized")
        => new(false, default, error, 401);

    public static Result<T> Forbidden(string error = "Access denied")
        => new(false, default, error, 403);

    // Implicit conversion from T
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>Non-generic Result for commands that return no data.</summary>
public class Result : Result<Unit>
{
    protected Result(bool isSuccess, string error, int errorCode = 0)
        : base(isSuccess, Unit.Value, error, errorCode) { }

    public static Result Success()             => new(true, string.Empty, 0);
    public static new Result Failure(string error, int errorCode = 400) => new(false, error, errorCode);
    public static new Result NotFound(string error = "Record not found") => new(false, error, 404);
    public static new Result Conflict(string error)                       => new(false, error, 409);
}

/// <summary>Empty return type for commands.</summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
