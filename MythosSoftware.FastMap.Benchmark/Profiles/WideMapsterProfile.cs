using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class WideMapsterProfile
{
    public static void Configure() => TypeAdapterConfig<WideSource, WideDestination>.NewConfig();
}