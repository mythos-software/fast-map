using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests.Profiles;

public class UserProfile: Profile
{
    public UserProfile()
    {
        CreateMap<User, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        
        CreateMap<StringUser, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<UserType>(src.Type)));

        CreateMap<User, StringUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        
        CreateMap<UserType, UserType>();
    }
}