using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests.Profiles;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<SimplePerson, SimplePerson>()
            .ForMember(src => src.Id, opt => opt.Ignore());
        
        CreateMap<SimplePerson, SimpleDestinationPerson>();
        
        CreateMap<SimplePerson, ModifiedSimplePerson>()
            .ForMember(src => src.FirstName, opt => opt.MapFrom(dest => dest.Name))
            .ForMember(src => src.LastName, opt => opt.MapFrom(dest => dest.Surname));
        
        CreateMap<SubclassPerson, SubclassDestinatonPerson>();
        
        CreateMap<CollectionPerson, CollectionDestinationPerson>();
        
        CreateMap<StringCollectionPerson, StringCollectionDestinationPerson>();
    }
}