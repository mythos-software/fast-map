using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.MappingProcessors;
using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for custom mapping resolvers
/// </summary>
public class ValueResolverTestsShould
{
    #region Resolvers

    class CustomResolver : IValueResolver<SimplePerson, SimplePerson, string>
    {
        public string Resolve(SimplePerson source, SimplePerson destination, string currentValue)
        {
            return $"{source.Id} {source.Name} {source.Surname}";
        }
    }

    #endregion

    #region Inline Test Profiles

    class SimplePersonCustomResolverTestProfile : Profile
    {
        public SimplePersonCustomResolverTestProfile()
        {
            CreateMap<SimplePerson, SimplePerson>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom<CustomResolver>());
        }
    }

    #endregion

    [Fact(Skip = "NotImplementedException")]
    public void MapCorrectlyUsingCustomResolver()
    {
        #region Arrange

        var serviceProvider = new ServiceCollection().AddFastMap(new SimplePersonCustomResolverTestProfile()).BuildServiceProvider();

        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name",
            Surname = "Surname"
        };

        #endregion

        #region Act

        var destination = mapper.Map<SimplePerson>(source);

        #endregion

        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be($"{source.Id} {source.Name} {source.Surname}");
        }

        #endregion
    }
}
