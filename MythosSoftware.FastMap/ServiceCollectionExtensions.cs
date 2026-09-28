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
        services.AddFastMapCommon();
        
        var profileTypes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => typeof(Profile).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var profileType in profileTypes)
        {
            var profileInstance = (Profile)Activator.CreateInstance(profileType);
        }
        
        return services;
    }

    public static IServiceCollection AddFastMap(this IServiceCollection services, params Profile[] profiles)
    {
        services.AddFastMapCommon();
        
        foreach (var profile in profiles)
        {
            var profileInstance = (Profile)Activator.CreateInstance(profile.GetType());
        }
        
        return services;
    }
    
    private static void AddFastMapCommon(this IServiceCollection services)
    {
        services.AddSingleton<IMapper, DefaultMapper>();
        
        RegisterSimpleType<string>();
        RegisterSimpleType<bool>();
        RegisterSimpleType<byte>();
        RegisterSimpleType<sbyte>();
        RegisterSimpleType<short>();
        RegisterSimpleType<ushort>();
        RegisterSimpleType<int>();
        RegisterSimpleType<uint>();
        RegisterSimpleType<long>();
        RegisterSimpleType<ulong>();
        RegisterSimpleType<float>();
        RegisterSimpleType<double>();
        RegisterSimpleType<decimal>();
        RegisterSimpleType<char>();
        RegisterSimpleType<DateTime>();
        RegisterSimpleType<DateTimeOffset>();
        RegisterSimpleType<TimeSpan>();
        RegisterSimpleType<Guid>();
    }
    
    private static void RegisterSimpleType<T>()
    {
        MappingProcessorBuilder<T, T>.Register(new SimpleTypeMappingProcessor<T, T>());
    }
}