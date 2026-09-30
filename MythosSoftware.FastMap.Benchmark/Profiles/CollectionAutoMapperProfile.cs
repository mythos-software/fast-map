using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class CollectionAutoMapperProfile : AutoMapper.Profile
{
    public CollectionAutoMapperProfile() => CreateMap<CollectionItemSource, CollectionItemDestination>();
}