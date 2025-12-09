namespace Fudie.Validation;

using FluentValidation;
using FluentValidation.Results;

public static class ValidationGuard
{
    public static void ThrowIf(bool condition, string message, string propertyName = "")
    {
        if (condition)
            throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }
    
    public static void ThrowIfNot(bool condition, string message, string propertyName = "")
    {
        if (!condition)
            throw new ValidationException([new ValidationFailure(propertyName, message)]);
    }
}