namespace Fudie.Domain;

using Fudie;
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

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Id == right.Id;
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    public static Result<T> ValidateEntity<T>(T entity, AbstractValidator<T> validator) where T : Entity
    {
        if (entity.Id == Guid.Empty)
        {
            return Result<T>.Failure("El id no puede estar vacío", nameof(Id));
        }

        var result = validator.Validate(entity);

        if (!result.IsValid)
        {
            var validationErrors = result.Errors.Select(e =>
                new ValidationError(e.ErrorMessage, e.PropertyName));

            return Result<T>.Failure(validationErrors);
        }

        return Result<T>.Success(entity);
    }
}
