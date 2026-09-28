using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing mappers in classes with subclass.
/// </summary>
public class SubclassMappingTestsShould
{
    [Fact]
    public void ShouldMapSourceToDestinationWhenObjectsHaveSubobjectsProperties()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new SubclassPerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName",
            Hobby = new SimpleHobby
            {
                Id = 1,
                Name = "HobbyName"
            }
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<SubclassDestinatonPerson>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be(source.Surname);
            destination.Hobby.Id.Should().Be(default);
            destination.Hobby.Name.Should().Be(source.Hobby.Name);
        }
        
        #endregion
    }
}