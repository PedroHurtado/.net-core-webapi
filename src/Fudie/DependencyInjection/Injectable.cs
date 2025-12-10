namespace Fudie.DependencyInjection;

public enum ServiceLifetime
{
    Transient,
    Scoped,
    Singleton
}



[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : Attribute
{
    public ServiceLifetime Lifetime { get; init; } = lifetime;
    public Type? ServiceType { get; init; }
}
