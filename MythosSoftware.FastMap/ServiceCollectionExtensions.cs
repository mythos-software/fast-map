using System.Reflection;
using MythosSoftware.FastMap;
using MythosSoftware.FastMap.MappingProcessors;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering FastMap services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFastMap(this IServiceCollection services, params Assembly[] assemblies)
    {
        var registry = CreateDefaultRegistry();
        
        var profileTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var profileType in profileTypes)
        {
            if (Activator.CreateInstance(profileType) is Profile profileInstance)
            {
                foreach (var processor in profileInstance.Processors)
                {
                    registry.Register(processor);
                }
            }
        }

        services.AddSingleton(registry);
        services.AddSingleton<IMapper, DefaultMapper>();
        
        return services;
    }

    public static IServiceCollection AddFastMap(this IServiceCollection services, params Profile[] profiles)
    {
        var registry = CreateDefaultRegistry();
        
        foreach (var profile in profiles)
        {
            foreach (var processor in profile.Processors)
            {
                registry.Register(processor);
            }
        }

        services.AddSingleton(registry);
        services.AddSingleton<IMapper, DefaultMapper>();
        
        return services;
    }
    
    private static MappingProcessorRegistry CreateDefaultRegistry()
    {
        var registry = new MappingProcessorRegistry();
        
        RegisterSimpleType<string>(registry);
        RegisterSimpleType<bool>(registry);
        RegisterSimpleType<byte>(registry);
        RegisterSimpleType<sbyte>(registry);
        RegisterSimpleType<short>(registry);
        RegisterSimpleType<ushort>(registry);
        RegisterSimpleType<int>(registry);
        RegisterSimpleType<uint>(registry);
        RegisterSimpleType<long>(registry);
        RegisterSimpleType<ulong>(registry);
        RegisterSimpleType<float>(registry);
        RegisterSimpleType<double>(registry);
        RegisterSimpleType<decimal>(registry);
        RegisterSimpleType<char>(registry);
        RegisterSimpleType<DateTime>(registry);
        RegisterSimpleType<DateTimeOffset>(registry);
        RegisterSimpleType<TimeSpan>(registry);
        RegisterSimpleType<Guid>(registry);

        return registry;
    }
    
    private static void RegisterSimpleType<T>(MappingProcessorRegistry registry)
    {
        registry.Register<T, T>(new SimpleTypeMappingProcessor<T, T>());
    }
}