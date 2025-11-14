namespace webapi.common.exceptions;

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
    
    public ValidationException(string error) 
        : base(error)
    {
        Errors = [error];
    }
    
    public ValidationException(IEnumerable<string> errors) 
        : base(string.Join(", ", errors))
    {
        Errors = errors;
    }
}