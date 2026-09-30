namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class BenchmarkFastMapProfile<TSource, TDestination> : Profile
{
    public BenchmarkFastMapProfile()
    {
        CreateMap<TSource, TDestination>();
    }
}