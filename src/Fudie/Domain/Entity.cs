namespace Fudie.Domain;

using FluentValidation;

public interface IEntity;

public abstract class Entity<TId>(TId id) : IEntity where TId : notnull
{
    public TId Id { get; init; } = id;

    public override bool Equals(object? obj)
    {
        if (obj is Entity<TId> entity)
        {
            return EqualityComparer<TId>.Default.Equals(entity.Id, Id);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(left.Id, right.Id);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    protected Result<T> ValidateEntity<T>(T entity, AbstractValidator<T> validator)
        where T : IEntity
    {
        if (Id is null || Id.Equals(default(TId)))
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