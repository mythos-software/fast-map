using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests.Profiles;

public class HobbyProfile : Profile
{
    public HobbyProfile()
    {
        CreateMap<SimpleHobby, SimpleHobby>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}