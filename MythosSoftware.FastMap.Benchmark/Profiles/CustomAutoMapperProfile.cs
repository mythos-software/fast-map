using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal sealed class CustomAutoMapperProfile : AutoMapper.Profile
{
    public CustomAutoMapperProfile()
    {
        CreateMap<CustomSource, CustomDestination>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}