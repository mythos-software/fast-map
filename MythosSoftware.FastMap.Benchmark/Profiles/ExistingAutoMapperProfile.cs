using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class ExistingAutoMapperProfile : AutoMapper.Profile
{
    public ExistingAutoMapperProfile() => CreateMap<ExistingSource, ExistingDestination>();
}