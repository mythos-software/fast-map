using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class CollectionMapsterProfile
{
    public static void Configure() => TypeAdapterConfig<CollectionItemSource, CollectionItemDestination>.NewConfig();
}