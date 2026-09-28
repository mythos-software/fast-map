using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests.Profiles;

public class HobbyProfile : Profile
{
    public HobbyProfile()
    {
        CreateMap<SimpleHobby, SimpleHobby>()
            .ForMember(src => src.Id, opt => opt.Ignore());
    }
}