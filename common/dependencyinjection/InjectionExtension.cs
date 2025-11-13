namespace webapi.common.dependencyinjection;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;




public static class InjectionExtension
{
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

            // Obtener solo las interfaces de primer nivel
            var topLevelInterfaces = GetTopLevelInterfaces(implementationType);

            foreach (var interfaceType in topLevelInterfaces)
            {
                // Verificar si ya está registrado
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
        var serviceDescriptor = lifetime switch
        {
            ServiceLifetime.Transient =>
                ServiceDescriptor.Transient(serviceType, implementationType),
            ServiceLifetime.Scoped =>
                ServiceDescriptor.Scoped(serviceType, implementationType),
            ServiceLifetime.Singleton =>
                ServiceDescriptor.Singleton(serviceType, implementationType),
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifetime),
                lifetime,
                "Lifetime no válido")
        };

        services.Add(serviceDescriptor);
    }
}