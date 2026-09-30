using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class ExistingFastMapProfile : Profile
{
    public ExistingFastMapProfile() => CreateMap<ExistingSource, ExistingDestination>();
}