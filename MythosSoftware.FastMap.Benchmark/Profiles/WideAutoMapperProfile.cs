using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class WideAutoMapperProfile : AutoMapper.Profile
{
    public WideAutoMapperProfile() => CreateMap<WideSource, WideDestination>();
}