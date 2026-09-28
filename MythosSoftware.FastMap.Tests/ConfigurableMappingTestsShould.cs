using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing configurable mappers (custom mapping configurations eg. .Ignore(), .MapFrom(), etc.).
/// </summary>
public class ConfigurableMappingTestsShould
{
    [Fact]
    public void ShouldPartiallyMapSourceToDestinationWhenObjectsHaveSameProperties()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SimplePerson
        {
            Id = 1,
            Name = "Name"
        };

        var destination = new SimplePerson
        {
            Id = 2,
            Name = "OldName",
            Surname = "LastName"
        };
        
        #endregion
        
        #region Act
        
        mapper.Map(source, destination);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(destination.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be(destination.Surname);
        }
        
        #endregion
    }
}