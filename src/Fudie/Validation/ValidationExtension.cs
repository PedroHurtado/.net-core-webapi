// Fudie.Validation/ValidatorExtensions.cs
namespace Fudie.Validation;

using FluentValidation;
using FluentValidation.Results;
using Fudie.Domain;

public static class ValidatorExtensions
{
    /// <summary>
    /// Valida cualquier objeto y lo retorna. Lanza ValidationException si falla.
    /// Para entidades con Id Guid, también valida que el Id no esté vacío.
    /// </summary>
    public static T ValidateOrThrow<T>(this IValidator<T> validator, T instance)
    {
        if (instance is Entity<Guid> entity && entity.Id == Guid.Empty)
        {
            throw new ValidationException([
                new ValidationFailure("Id", "El id no puede estar vacío")
            ]);
        }

        var result = validator.Validate(instance);

        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return instance;
    }
}
