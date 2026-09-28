using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing reverse mapping functionality in the FastMap library.
/// </summary>
public class ReverseMappingTestsShould
{
    #region Inline Test Profiles 
    
    class OneWayTestProfile : Profile
    {
        public OneWayTestProfile()
        {
            CreateMap<SimplePerson, ModifiedSimplePerson>()
                .ForMember(src => src.FirstName, opt => opt.MapFrom(dest => dest.Name))
                .ForMember(src => src.LastName, opt => opt.MapFrom(dest => dest.Surname));
        }
    }
    
    class TwoWaySimpleTestProfile : Profile
    {
        public TwoWaySimpleTestProfile()
        {
            CreateMap<SimplePerson, SimpleDestinationPerson>()
                .ForMember(src => src.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
    
    class TwoWayTestProfile : Profile
    {
        public TwoWayTestProfile()
        {
            CreateMap<SimplePerson, ModifiedSimplePerson>()
                .ForMember(src => src.FirstName, opt => opt.MapFrom(dest => dest.Name))
                .ForMember(src => src.LastName, opt => opt.MapFrom(dest => dest.Surname))
                .ReverseMap();
        }
    }
    
    #endregion
    
    [Fact]
    public void ThrowExceptionWhenReverseMapIsNotConfigured()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new OneWayTestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName"
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<ModifiedSimplePerson>(source);
        
        Action act = () => mapper.Map<SimplePerson>(destination);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            act.Should().Throw<InvalidOperationException>();
        }
        
        #endregion
    }
    
    [Fact]
    public void MapCorrectlyBothWaysWhenReverseMapIsConfigured()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TwoWaySimpleTestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName"
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<SimpleDestinationPerson>(source);
        
        var destination2 = mapper.Map<SimplePerson>(destination);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination2.Name.Should().BeEquivalentTo(source.Name);
            destination2.Surname.Should().BeEquivalentTo(source.Surname);
        }
        
        #endregion
    }
    
    [Fact]
    public void MapCorrectlyBothWaysWhenReverseMapIsConfiguredAndPropertyNamesDiffer()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TwoWayTestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName"
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<ModifiedSimplePerson>(source);
        
        var destination2 = mapper.Map<SimplePerson>(destination);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination2.Should().BeEquivalentTo(source);
        }
        
        #endregion
    }
}