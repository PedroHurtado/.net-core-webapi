namespace Fudie;

using FluentValidation;
using FluentValidation.Results;

public static class ResultExtensions
{
    public static T ValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            var failures = result.Errors.Select(e => 
                new ValidationFailure(e.PropertyName, e.ErrorMessage));
            
            throw new ValidationException(failures);
        }
        
        return result.Value!;
    }
    
    public static void SuccessOrThrow(this Result result)
    {
        if (result.IsFailure)
        {
            var failures = result.Errors.Select(e => 
                new ValidationFailure(e.PropertyName, e.ErrorMessage));
            
            throw new ValidationException(failures);
        }
    }
}
