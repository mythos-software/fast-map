using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class NestedMapsterProfile
{
    public static void Configure()
    {
        TypeAdapterConfig<NestedSource, NestedDestination>.NewConfig();
        TypeAdapterConfig<NestedCustomerSource, NestedCustomerDestination>.NewConfig();
        TypeAdapterConfig<NestedAddressSource, NestedAddressDestination>.NewConfig();
    }
}