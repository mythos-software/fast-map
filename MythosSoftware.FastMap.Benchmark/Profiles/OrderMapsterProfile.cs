using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class OrderMapsterProfile
{
    public static void Configure()
    {
        TypeAdapterConfig<OrderSource, OrderDestination>.NewConfig();
        TypeAdapterConfig<OrderCustomerSource, OrderCustomerDestination>.NewConfig();
        TypeAdapterConfig<OrderAddressSource, OrderAddressDestination>.NewConfig();
        TypeAdapterConfig<OrderLineSource, OrderLineDestination>.NewConfig();
    }
}