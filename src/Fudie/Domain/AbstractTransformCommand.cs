namespace Fudie.Domain;

/// <summary>
/// Comando de dominio abstracto para transformar un value object existente con datos.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando con los datos de transformación.</typeparam>
/// <typeparam name="TValueObject">Tipo del value object a transformar.</typeparam>
public abstract class AbstractTransformCommand<TCommand, TValueObject>
    where TCommand : class
    where TValueObject : class
{
    /// <summary>
    /// Ejecuta la transformación del value object.
    /// </summary>
    /// <param name="current">Value object existente a transformar.</param>
    /// <param name="command">Datos para la transformación.</param>
    /// <returns>El value object transformado.</returns>
    public abstract TValueObject Execute(TValueObject current, TCommand command);
}

/// <summary>
/// Comando de dominio abstracto para transformar un value object existente sin datos adicionales.
/// </summary>
/// <typeparam name="TValueObject">Tipo del value object a transformar.</typeparam>
public abstract class AbstractTransformCommand<TValueObject>
    where TValueObject : class
{
    /// <summary>
    /// Ejecuta la transformación del value object.
    /// </summary>
    /// <param name="current">Value object existente a transformar.</param>
    /// <returns>El value object transformado.</returns>
    public abstract TValueObject Execute(TValueObject current);
}
