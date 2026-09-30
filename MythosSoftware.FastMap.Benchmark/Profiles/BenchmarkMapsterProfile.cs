using Mapster;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class BenchmarkMapsterProfile<TSource, TDestination>
{
    public static void Configure()
    {
        TypeAdapterConfig<TSource, TDestination>.NewConfig();
    }
}