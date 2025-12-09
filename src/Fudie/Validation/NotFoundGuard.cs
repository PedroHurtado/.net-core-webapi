// Fudie.Validation/NotFoundGuard.cs
namespace Fudie.Validation;

public static class NotFoundGuard
{
    public static T ThrowIfNull<T>(T? entity, string message) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException(message);
        
        return entity;
    }
}