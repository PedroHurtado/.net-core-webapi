namespace webapi.common.dependencyinjection;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public static class InjectionExtension
{
    /// <summary>
    /// Registra automáticamente todas las clases decoradas con [Injectable]
    /// </summary>
    public static IServiceCollection AddInjectables(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var injectableTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttribute<InjectableAttribute>() is not null
                        && type is { IsClass: true, IsAbstract: false });

        foreach (var implementationType in injectableTypes)
        {
            var attribute = implementationType.GetCustomAttribute<InjectableAttribute>()!;
            var topLevelInterfaces = GetTopLevelInterfaces(implementationType);

            foreach (var interfaceType in topLevelInterfaces)
            {
                if (!IsServiceRegistered(services, interfaceType))
                {
                    RegisterService(services, interfaceType, implementationType, attribute.Lifetime);
                }
            }

            // Si no tiene interfaces, registrar la clase misma
            if (topLevelInterfaces.Count == 0 && !IsServiceRegistered(services, implementationType))
            {
                RegisterService(services, implementationType, implementationType, attribute.Lifetime);
            }
        }

        return services;
    }

    /// <summary>
    /// Registra las interfaces implementadas por un tipo ya registrado en el contenedor
    /// </summary>
    public static IServiceCollection AddInterfacesFor<TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var implementationType = typeof(TImplementation);
        
        if (!IsServiceRegistered(services, implementationType))
        {
            throw new InvalidOperationException(
                $"Cannot register interfaces for {implementationType.Name} because it's not registered in the container. " +
                $"Please register {implementationType.Name} before calling AddInterfacesFor.");
        }

        var topLevelInterfaces = GetTopLevelInterfaces(implementationType);

        if (topLevelInterfaces.Count == 0)
        {
            return services;
        }

        foreach (var interfaceType in topLevelInterfaces)
        {
            if (!IsServiceRegistered(services, interfaceType))
            {
                RegisterServiceFromFactory(
                    services, 
                    interfaceType, 
                    sp => sp.GetRequiredService<TImplementation>(), 
                    lifetime);
            }
        }

        return services;
    }

    private static HashSet<Type> GetTopLevelInterfaces(Type type)
    {
        var allInterfaces = type.GetInterfaces();
        var topLevelInterfaces = new HashSet<Type>();

        foreach (var interfaceType in allInterfaces)
        {
            // Verificar si esta interfaz es heredada por alguna otra interfaz que la clase implementa
            bool isInheritedByOther = allInterfaces.Any(other =>
                other != interfaceType && other.GetInterfaces().Contains(interfaceType));

            // Si no es heredada por otra, es de primer nivel
            if (!isInheritedByOther)
            {
                topLevelInterfaces.Add(interfaceType);
            }
        }

        return topLevelInterfaces;
    }

    private static bool IsServiceRegistered(IServiceCollection services, Type serviceType)
    {
        return services.Any(descriptor => descriptor.ServiceType == serviceType);
    }

    private static void RegisterService(
        IServiceCollection services,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        var serviceDescriptor = CreateServiceDescriptor(serviceType, implementationType, lifetime);
        services.Add(serviceDescriptor);
    }

    private static void RegisterServiceFromFactory(
        IServiceCollection services,
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime)
    {
        var serviceDescriptor = CreateServiceDescriptor(serviceType, implementationFactory, lifetime);
        services.Add(serviceDescriptor);
    }

    private static ServiceDescriptor CreateServiceDescriptor(
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => ServiceDescriptor.Transient(serviceType, implementationType),
            ServiceLifetime.Scoped => ServiceDescriptor.Scoped(serviceType, implementationType),
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton(serviceType, implementationType),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime")
        };
    }

    private static ServiceDescriptor CreateServiceDescriptor(
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime)
    {
        return lifetime switch
        {
            ServiceLifetime.Transient => ServiceDescriptor.Transient(serviceType, implementationFactory),
            ServiceLifetime.Scoped => ServiceDescriptor.Scoped(serviceType, implementationFactory),
            ServiceLifetime.Singleton => ServiceDescriptor.Singleton(serviceType, implementationFactory),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Invalid service lifetime")
        };
    }
}