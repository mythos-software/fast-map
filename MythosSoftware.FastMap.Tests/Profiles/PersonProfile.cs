using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests.Profiles;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<SimplePerson, SimplePerson>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        
        CreateMap<SimplePerson, SimpleDestinationPerson>();
        
        CreateMap<SimplePerson, ModifiedSimplePerson>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname));
        
        CreateMap<SubclassPerson, SubclassDestinatonPerson>();
        
        CreateMap<CollectionPerson, CollectionDestinationPerson>();
        
        CreateMap<StringCollectionPerson, StringCollectionDestinationPerson>();
    }
}