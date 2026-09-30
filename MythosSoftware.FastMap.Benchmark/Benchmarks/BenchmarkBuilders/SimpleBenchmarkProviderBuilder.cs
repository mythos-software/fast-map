using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;

internal sealed class SimpleBenchmarkProviderBuilder<TSource, TDestination>
{
    public ServiceProvider Build()
    {
        var services = new ServiceCollection();

        services.AddFastMap(new BenchmarkFastMapProfile<TSource, TDestination>());
        
        var mapconfig = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new BenchmarkAutomapperProfile<TSource, TDestination>());
        });
        services.AddSingleton(mapconfig.CreateMapper());
        
        BenchmarkMapsterProfile<TSource, TDestination>.Configure();

        return services.BuildServiceProvider();
    }
}