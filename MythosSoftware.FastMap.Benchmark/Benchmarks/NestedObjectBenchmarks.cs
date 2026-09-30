using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class NestedObjectBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private NestedSource _source = null!;
    
    #endregion

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new NestedFastMapProfile(), new NestedAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        NestedMapsterProfile.Configure();

        _source = new NestedSource
        {
            Id = 42,
            Customer = new NestedCustomerSource
            {
                Name = "Ada Lovelace",
                Address = new NestedAddressSource
                {
                    Street = "1 Benchmark Way",
                    City = "London",
                    Country = "UK"
                }
            }
        };
    }

    [Benchmark(Baseline = true)]
    public NestedDestination Manual() => new()
    {
        Id = _source.Id,
        Customer = new NestedCustomerDestination
        {
            Name = _source.Customer.Name,
            Address = new NestedAddressDestination
            {
                Street = _source.Customer.Address.Street,
                City = _source.Customer.Address.City,
                Country = _source.Customer.Address.Country
            }
        }
    };

    [Benchmark]
    public NestedDestination FastMap() => _fastMapper.Map<NestedDestination>(_source);

    [Benchmark]
    public NestedDestination AutoMapper() => _autoMapper.Map<NestedDestination>(_source);

    [Benchmark]
    public NestedDestination Mapster() => _source.Adapt<NestedDestination>();
}
