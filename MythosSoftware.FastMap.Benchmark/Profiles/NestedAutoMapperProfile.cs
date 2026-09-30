using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class NestedAutoMapperProfile : AutoMapper.Profile
{
    public NestedAutoMapperProfile()
    {
        CreateMap<NestedSource, NestedDestination>();
        CreateMap<NestedCustomerSource, NestedCustomerDestination>();
        CreateMap<NestedAddressSource, NestedAddressDestination>();
    }
}