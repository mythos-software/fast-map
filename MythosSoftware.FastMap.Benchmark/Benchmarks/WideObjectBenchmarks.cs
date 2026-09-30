using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class WideObjectBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private WideSource _source = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new WideFastMapProfile(), new WideAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        WideMapsterProfile.Configure();

        _source = new WideSource
        {
            P01 = 1, P02 = 2, P03 = 3, P04 = 4, P05 = 5,
            P06 = 6, P07 = 7, P08 = 8, P09 = 9, P10 = 10,
            P11 = 11, P12 = 12, P13 = 13.13m, P14 = 14.14m,
            P15 = true, P16 = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            P17 = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            P18 = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            P19 = "nineteen", P20 = "twenty", P21 = null, P22 = "twenty-two",
            P23 = 23.23, P24 = 24.24f, P25 = 25
        };
    }

    [Benchmark(Baseline = true)]
    public WideDestination Manual() => new()
    {
        P01 = _source.P01, P02 = _source.P02, P03 = _source.P03, P04 = _source.P04, P05 = _source.P05,
        P06 = _source.P06, P07 = _source.P07, P08 = _source.P08, P09 = _source.P09, P10 = _source.P10,
        P11 = _source.P11, P12 = _source.P12, P13 = _source.P13, P14 = _source.P14, P15 = _source.P15,
        P16 = _source.P16, P17 = _source.P17, P18 = _source.P18, P19 = _source.P19, P20 = _source.P20,
        P21 = _source.P21, P22 = _source.P22, P23 = _source.P23, P24 = _source.P24, P25 = _source.P25
    };

    [Benchmark]
    public WideDestination FastMap() => _fastMapper.Map<WideDestination>(_source);

    [Benchmark]
    public WideDestination AutoMapper() => _autoMapper.Map<WideDestination>(_source);

    [Benchmark]
    public WideDestination Mapster() => _source.Adapt<WideDestination>();
}
