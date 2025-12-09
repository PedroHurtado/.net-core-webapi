namespace Fudie.Validation;

public static class NotFoundGuard
{
    /// <summary>
    /// Lanza si es null. Mensaje: "{TypeName} no encontrado"
    /// </summary>
    public static T ThrowIfNull<T>(T? entity) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(T).Name} no encontrado");
        
        return entity;
    }

    /// <summary>
    /// Lanza si es null. Mensaje: "{TypeName} con Id '{id}' no encontrado"
    /// </summary>
    public static T ThrowIfNull<T, TId>(T? entity, TId id) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException($"{typeof(T).Name} con Id '{id}' no encontrado");
        
        return entity;
    }

    /// <summary>
    /// Lanza si es null con mensaje personalizado.
    /// </summary>
    public static T ThrowIfNull<T>(T? entity, string message) where T : class
    {
        if (entity is null)
            throw new KeyNotFoundException(message);
        
        return entity;
    }
}