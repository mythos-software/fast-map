using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class NestedFastMapProfile : Profile
{
    public NestedFastMapProfile()
    {
        CreateMap<NestedSource, NestedDestination>();
        CreateMap<NestedCustomerSource, NestedCustomerDestination>();
        CreateMap<NestedAddressSource, NestedAddressDestination>();
    }
}