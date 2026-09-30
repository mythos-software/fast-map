namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class BenchmarkAutomapperProfile<TSource, TDestination> : AutoMapper.Profile
{
    public BenchmarkAutomapperProfile()
    {
        CreateMap<TSource, TDestination>();
    }
}