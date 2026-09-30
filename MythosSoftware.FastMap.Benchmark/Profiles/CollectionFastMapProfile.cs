using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class CollectionFastMapProfile : Profile
{
    public CollectionFastMapProfile() => CreateMap<CollectionItemSource, CollectionItemDestination>();
}