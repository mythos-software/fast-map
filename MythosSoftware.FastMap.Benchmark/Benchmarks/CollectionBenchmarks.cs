using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class CollectionBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private List<CollectionItemSource> _source = null!;
    
    #endregion

    [Params(10, 100, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new CollectionFastMapProfile(), new CollectionAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        CollectionMapsterProfile.Configure();

        _source = Enumerable.Range(1, Count)
            .Select(i => new CollectionItemSource
            {
                Id = i,
                Name = $"Item {i}",
                Amount = i * 1.25m,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            })
            .ToList();
    }

    [Benchmark(Baseline = true)]
    public List<CollectionItemDestination> Manual()
    {
        var result = new List<CollectionItemDestination>(_source.Count);
        foreach (var item in _source)
        {
            result.Add(new CollectionItemDestination
            {
                Id = item.Id,
                Name = item.Name,
                Amount = item.Amount,
                CreatedAt = item.CreatedAt
            });
        }
        return result;
    }

    [Benchmark]
    public List<CollectionItemDestination> FastMap() => _fastMapper.Map<List<CollectionItemDestination>>(_source);

    [Benchmark]
    public List<CollectionItemDestination> AutoMapper() => _autoMapper.Map<List<CollectionItemDestination>>(_source);

    [Benchmark]
    public List<CollectionItemDestination> Mapster() => _source.Adapt<List<CollectionItemDestination>>();
}
