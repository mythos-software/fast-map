using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class ExistingMapsterProfile
{
    public static void Configure() => TypeAdapterConfig<ExistingSource, ExistingDestination>.NewConfig();
}