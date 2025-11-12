namespace webapi.common.domain;

using webapi.common;
using FluentValidation;

public abstract class Entity(Guid id)
{
    public Guid Id { get; protected set; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is Entity entity)
        {
            return entity.Id == Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected static Result ValidateEntity<T>(T entity, AbstractValidator<T> validator) where T : Entity
    {
        // Primero validar el Id (común para todas las entidades)
        if (entity.Id == Guid.Empty)
        {
            return Result.Failure("El id no puede estar vacío");
        }

        // Luego validar las reglas específicas de la entidad
        var result = validator.Validate(entity);

        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage);
            return Result.Failure(errors);
        }

        return Result.Success();
    }
}