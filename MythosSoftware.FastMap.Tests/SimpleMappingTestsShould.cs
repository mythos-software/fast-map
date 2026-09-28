using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing simple mappers (eg. primitive types, strings, etc.).
/// </summary>
public class SimpleMappingTestsShould
{
    #region Inline Test Profiles 
    
    class TestProfile : Profile
    {
        public TestProfile()
        {
            CreateMap<SimplePerson, ModifiedSimpleHobby>();
        }
    }
    
    #endregion
    
    [Fact]
    public void ShouldMapSourceToDestinationWhenObjectsHaveSameProperties()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
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
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be(source.Surname);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapSourceToDestinationWhenObjectsHaveDifferentProperties()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
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
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.FirstName.Should().Be(source.Name);
            destination.LastName.Should().Be(source.Surname);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapDefaultWhenSourceAndDestinationObjectsHaveDifferentProperties()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName"
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<ModifiedSimpleHobby>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.ActivityName.Should().Be(default);
        }
        
        #endregion
    }

    [Fact]
    public void ShouldMapSimpleTypeStringToSimpleType()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = "test";

        #endregion
        
        #region Act
        
        var destination = mapper.Map<string>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapSimpleTypeDateTimeToSimpleType()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = DateTime.Now;

        #endregion
        
        #region Act
        
        var destination = mapper.Map<DateTime>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapSimpleTypeDateTimeOffsetToSimpleType()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = DateTimeOffset.Now;

        #endregion
        
        #region Act
        
        var destination = mapper.Map<DateTimeOffset>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapSimpleTypeDecimalToSimpleType()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = 123.45m;

        #endregion
        
        #region Act
        
        var destination = mapper.Map<decimal>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }
    
    [Fact]
    public void ShouldMapSimpleTypeDoubleToSimpleType()
    {
        #region Arrange
        
        var serviceProvider = new ServiceCollection().AddFastMap(new TestProfile()).BuildServiceProvider();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = 123.45;

        #endregion
        
        #region Act
        
        var destination = mapper.Map<double>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }
}