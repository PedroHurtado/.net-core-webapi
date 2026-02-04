namespace Fudie.DependencyInjection;

using System.Reflection;
using FluentValidation;
using Fudie.Domain;
using Microsoft.Extensions.DependencyInjection;

public static class DomainCommandsExtension
{
    public static IServiceCollection AddDomainCommands(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            if (IsSubclassOfGeneric(type, typeof(AbstractValidator<>)) ||
                IsSubclassOfGeneric(type, typeof(AbstractCreateCommand<,>)) ||
                IsSubclassOfGeneric(type, typeof(AbstractModifyCommand<>)) ||
                IsSubclassOfGeneric(type, typeof(AbstractModifyCommand<,>)))
            {
                services.AddSingleton(type);
            }
        }

        return services;
    }

    private static bool IsSubclassOfGeneric(Type type, Type genericBase)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase)
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}