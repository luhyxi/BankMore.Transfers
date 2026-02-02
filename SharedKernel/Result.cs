namespace SharedKernel;

public interface IResult
{
    bool IsSuccess { get; }
    string? Error { get; }
}

public interface IResult<out T> : IResult
{
    T? Value { get; }
}

public sealed record Result(bool IsSuccess, string? Error) : IResult
{
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public sealed record Result<T>(T? Value, bool IsSuccess, string? Error) : IResult<T>
{
    public static Result<T> Success(T value) => new(value, true, null);
    public static Result<T> Failure(string error) => new(default, false, error);
}
