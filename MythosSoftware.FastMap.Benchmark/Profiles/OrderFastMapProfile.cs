using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class OrderFastMapProfile : Profile
{
    public OrderFastMapProfile()
    {
        CreateMap<OrderSource, OrderDestination>();
        CreateMap<OrderCustomerSource, OrderCustomerDestination>();
        CreateMap<OrderAddressSource, OrderAddressDestination>();
        CreateMap<OrderLineSource, OrderLineDestination>();
    }
}