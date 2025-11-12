namespace webapi.common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool isSuccess, string? error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? (error != null ? [error] : Array.Empty<string>());
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) 
        => new(false, string.Join(", ", errors), errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(T? value, bool isSuccess, string? error, IEnumerable<string>? errors = null) 
        : base(isSuccess, error, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null);
    public new static Result<T> Failure(string error) => new(default, false, error);
    public new static Result<T> Failure(IEnumerable<string> errors) 
        => new(default, false, string.Join(", ", errors), errors);
}