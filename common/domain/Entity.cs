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
        if (entity.Id == Guid.Empty)
        {
            return Result.Failure("El id no puede estar vacío", nameof(Id));
        }

        var result = validator.Validate(entity);

        if (!result.IsValid)
        {
            var validationErrors = result.Errors.Select(e => 
                new ValidationError(e.ErrorMessage, e.PropertyName));
            
            return Result.Failure(validationErrors);
        }

        return Result.Success();
    }
}