using Mapster;
using MythosSoftware.FastMap.Benchmark.Objects;

namespace MythosSoftware.FastMap.Benchmark.Profiles;

internal static class CustomMapsterProfile
{
    public static void Configure()
    {
        TypeAdapterConfig<CustomSource, CustomDestination>
            .NewConfig()
            .Map(dest => dest.FullName, src => src.FirstName + " " + src.LastName)
            .Map(dest => dest.StatusText, src => src.Status.ToString());
    }
}