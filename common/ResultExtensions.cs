namespace webapi.common;

using webapi.common.exceptions;

public static class ResultExtensions
{
    /// <summary>
    /// Devuelve el valor o lanza ValidationException si hay error
    /// </summary>
    public static T ValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            throw new ValidationException(result.Errors);
        }
        
        return result.Value!;
    }
    
    /// <summary>
    /// Ejecuta una acción si el resultado es exitoso, lanza excepción si hay error
    /// </summary>
    public static void SuccessOrThrow(this Result result)
    {
        if (result.IsFailure)
        {
            throw new ValidationException(result.Errors);
        }
    }
}