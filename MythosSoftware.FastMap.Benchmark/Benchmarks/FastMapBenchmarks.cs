using BenchmarkDotNet.Attributes;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class FastMapBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private SimpleSourceObject _source = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        var provider = new SimpleBenchmarkProviderBuilder<SimpleSourceObject, SimpleDestinationObject>().Build();
        _fastMapper = provider.GetRequiredService<IMapper>();
        _autoMapper = provider.GetRequiredService<AutoMapper.IMapper>();
        
        _source = new SimpleSourceObject
        {
            Id = 1,
            Name = "John Doe",
            Age = 30
        };
    }

    [Benchmark(Baseline = true)]
    public SimpleDestinationObject Manual()
    {
        return new SimpleDestinationObject
        {
            Id = _source.Id,
            Name = _source.Name,
            Age = _source.Age
        };
    }

    [Benchmark]
    public SimpleDestinationObject FastMap()
    {
        return _fastMapper.Map<SimpleDestinationObject>(_source);
    }

    [Benchmark]
    public SimpleDestinationObject AutoMapper()
    {
        return _autoMapper.Map<SimpleDestinationObject>(_source);
    }

    [Benchmark]
    public SimpleDestinationObject Mapster()
    {
        return _source.Adapt<SimpleDestinationObject>();
    }
}