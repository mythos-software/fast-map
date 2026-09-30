using BenchmarkDotNet.Attributes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class ColdStartBenchmarks
{
    #region Fields
    
    private WideSource _source = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        _source = new WideSource { P01 = 1, P19 = "cold-start", P20 = "test" };
    }

    [Benchmark]
    public WideDestination FastMap_CreateConfigureResolveAndFirstMap()
    {
        var services = new ServiceCollection();
        services.AddFastMap(new WideFastMapProfile());
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        return mapper.Map<WideDestination>(_source);
    }

    [Benchmark]
    public WideDestination AutoMapper_CreateConfigureAndFirstMap()
    {
        var config = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile(new WideAutoMapperProfile()));
        var mapper = config.CreateMapper();
        return mapper.Map<WideDestination>(_source);
    }

    [Benchmark]
    public WideDestination Mapster_CreateConfigureAndFirstMap()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<WideSource, WideDestination>();
        return _source.Adapt<WideDestination>(config);
    }
}