using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class ExistingDestinationBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private ExistingSource _source = null!;
    private ExistingDestination _manualDestination = null!;
    private ExistingDestination _fastDestination = null!;
    private ExistingDestination _autoDestination = null!;
    private ExistingDestination _mapsterDestination = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new ExistingFastMapProfile(), new ExistingAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        ExistingMapsterProfile.Configure();

        _source = new ExistingSource { Id = 7, Name = "Updated", Age = 33 };
        _manualDestination = NewDestination();
        _fastDestination = NewDestination();
        _autoDestination = NewDestination();
        _mapsterDestination = NewDestination();
    }

    private static ExistingDestination NewDestination() => new() { Id = -1, Name = "Old", Age = -1 };

    [Benchmark(Baseline = true)]
    public ExistingDestination Manual()
    {
        _manualDestination.Id = _source.Id;
        _manualDestination.Name = _source.Name;
        _manualDestination.Age = _source.Age;
        return _manualDestination;
    }

    [Benchmark]
    public ExistingDestination FastMap() =>
        _fastMapper.Map<ExistingSource, ExistingDestination>(_source, _fastDestination);

    [Benchmark]
    public ExistingDestination AutoMapper() => _autoMapper.Map(_source, _autoDestination);

    [Benchmark]
    public ExistingDestination Mapster() => _source.Adapt(_mapsterDestination);
}
