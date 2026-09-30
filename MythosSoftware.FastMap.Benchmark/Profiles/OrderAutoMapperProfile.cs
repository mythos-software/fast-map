using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class OrderAutoMapperProfile : AutoMapper.Profile
{
    public OrderAutoMapperProfile()
    {
        CreateMap<OrderSource, OrderDestination>();
        CreateMap<OrderCustomerSource, OrderCustomerDestination>();
        CreateMap<OrderAddressSource, OrderAddressDestination>();
        CreateMap<OrderLineSource, OrderLineDestination>();
    }
}