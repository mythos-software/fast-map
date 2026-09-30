using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class CustomMappingBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private CustomSource _source = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new CustomFastMapProfile(), new CustomAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        CustomMapsterProfile.Configure();

        _source = new CustomSource
        {
            Id = 99,
            FirstName = "Grace",
            LastName = "Hopper",
            Status = BenchmarkOrderStatus.Shipped
        };
    }

    [Benchmark(Baseline = true)]
    public CustomDestination Manual() => new()
    {
        Id = _source.Id,
        FullName = _source.FirstName + " " + _source.LastName,
        StatusText = _source.Status.ToString()
    };

    [Benchmark]
    public CustomDestination FastMap() => _fastMapper.Map<CustomDestination>(_source);

    [Benchmark]
    public CustomDestination AutoMapper() => _autoMapper.Map<CustomDestination>(_source);

    [Benchmark]
    public CustomDestination Mapster() => _source.Adapt<CustomDestination>();
}
