using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class WideFastMapProfile : Profile
{
    public WideFastMapProfile() => CreateMap<WideSource, WideDestination>();
}