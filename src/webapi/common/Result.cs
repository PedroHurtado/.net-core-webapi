namespace webapi.common;

public class ValidationError(string errorMessage, string propertyName = "")
{
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IEnumerable<ValidationError> Errors { get; }

    protected Result(bool isSuccess, IEnumerable<ValidationError>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<ValidationError>();
    }

    public static Result Success() => new(true);
    
    public static Result Failure(string error, string propertyName = "") 
        => new(false, [new ValidationError(error, propertyName)]);
    
    public static Result Failure(IEnumerable<ValidationError> errors) 
        => new(false, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(T? value, bool isSuccess, IEnumerable<ValidationError>? errors = null) 
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true);
    
    public new static Result<T> Failure(string error, string propertyName = "") 
        => new(default, false, [new ValidationError(error, propertyName)]);
    
    public new static Result<T> Failure(IEnumerable<ValidationError> errors) 
        => new(default, false, errors);
}