using BenchmarkDotNet.Attributes;
using Mapster;
using MythosSoftware.FastMap.Benchmark.Benchmarks.BenchmarkBuilders;
using MythosSoftware.FastMap.Benchmark.Objects;
using MythosSoftware.FastMap.Benchmark.Profiles;

namespace MythosSoftware.FastMap.Benchmark.Benchmarks;

[MemoryDiagnoser]
public class RealWorldOrderBenchmarks
{
    #region Fields
    
    private IMapper _fastMapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private OrderSource _source = null!;
    
    #endregion

    [Params(10, 100)]
    public int LineCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var mappers = new SimpleBenchmarkProviderBuilder<object, object>().Build(new OrderFastMapProfile(), new OrderAutoMapperProfile());
        _fastMapper = mappers.FastMap;
        _autoMapper = mappers.AutoMapper;
        OrderMapsterProfile.Configure();

        _source = new OrderSource
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Number = "ORD-2026-0001",
            CreatedAt = new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero),
            Total = LineCount * 19.95m,
            Status = BenchmarkOrderStatus.Paid,
            Customer = new OrderCustomerSource
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Benchmark Customer",
                Address = new OrderAddressSource
                {
                    Street = "42 Mapper Street",
                    City = "Zagreb",
                    Country = "Croatia"
                }
            },
            Lines = Enumerable.Range(1, LineCount)
                .Select(i => new OrderLineSource
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = $"Product {i}",
                    Quantity = (i % 4) + 1,
                    UnitPrice = 19.95m
                })
                .ToList()
        };
    }

    [Benchmark(Baseline = true)]
    public OrderDestination Manual() => new()
    {
        Id = _source.Id,
        Number = _source.Number,
        CreatedAt = _source.CreatedAt,
        Total = _source.Total,
        Status = _source.Status,
        Customer = new OrderCustomerDestination
        {
            Id = _source.Customer.Id,
            Name = _source.Customer.Name,
            Address = _source.Customer.Address is null ? null : new OrderAddressDestination
            {
                Street = _source.Customer.Address.Street,
                City = _source.Customer.Address.City,
                Country = _source.Customer.Address.Country
            }
        },
        Lines = _source.Lines.Select(x => new OrderLineDestination
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice
        }).ToList()
    };

    [Benchmark]
    public OrderDestination FastMap() => _fastMapper.Map<OrderDestination>(_source);

    [Benchmark]
    public OrderDestination AutoMapper() => _autoMapper.Map<OrderDestination>(_source);

    [Benchmark]
    public OrderDestination Mapster() => _source.Adapt<OrderDestination>();
}
