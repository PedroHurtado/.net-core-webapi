namespace webapi.common.dependencyinjection;

public enum ServiceLifetime
{
    Transient,
    Scoped,
    Singleton
}



[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : Attribute
{
    public ServiceLifetime Lifetime { get; init; } = lifetime;
    public Type? ServiceType { get; init; }
}