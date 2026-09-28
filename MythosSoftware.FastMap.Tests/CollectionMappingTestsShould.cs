using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing collection mappers (eg. List, Dictionary, etc.).
/// </summary>
public class CollectionMappingTestsShould
{
    [Fact]
    public void MapSourceListToDestinationListWhenObjectsHaveSameProperties()
    {
        #region Arrange

        var serviceProvider = new TestServiceProvider().CreateServices();

        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = new List<SimplePerson>
        {
            new()
            {
                Id = 1,
                Name = "Name",
                Surname = "LastName"
            }
        };

        #endregion

        #region Act

        var destination = mapper.Map<List<SimpleDestinationPerson>>(source);

        #endregion

        #region Assert

        using (new AssertionScope())
        {
            destination.Count.Should().Be(source.Count);
            destination.First().Id.Should().Be(source.First().Id);
            destination.First().Name.Should().Be(source.First().Name);
            destination.First().Surname.Should().Be(source.First().Surname);
        }

        #endregion
    }

    [Fact]
    public void MapSourceSubobjectWithListToDestinationSubobjectWithListWhenObjectsHaveSameProperties()
    {
        #region Arrange

        var serviceProvider = new TestServiceProvider().CreateServices();

        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = new CollectionPerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName",
            Hobbies = new List<SimpleHobby>
            {
                new()
                {
                    Id = 1,
                    Name = "HobbyName"
                }
            }
        };

        #endregion

        #region Act

        var destination = mapper.Map<CollectionDestinationPerson>(source);

        #endregion

        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be(source.Surname);
            destination.Hobbies.Count.Should().Be(source.Hobbies.Count);
            destination.Hobbies.First().Id.Should().Be(default);
            destination.Hobbies.First().Name.Should().Be(source.Hobbies.First().Name);
        }

        #endregion
    }
    
    [Fact]
    public void MapSourceListToDestinationListWhenObjectsHaveDifferentProperties()
    {
        #region Arrange

        var serviceProvider = new TestServiceProvider().CreateServices();

        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new List<SimplePerson>
        {
            new()
            {
                Id = 1,
                Name = "Name",
                Surname = "LastName"
            }
        };

        #endregion

        #region Act

        var destination = mapper.Map<List<ModifiedSimplePerson>>(source);

        #endregion

        #region Assert

        using (new AssertionScope())
        {
            destination.First().Id.Should().Be(source.First().Id);
            destination.First().FirstName.Should().Be(source.First().Name);
            destination.First().LastName.Should().Be(source.First().Surname);
        }

        #endregion
    }
    
    [Fact]
    public void MapSourceSubobjectWithStringListToDestinationSubobjectWithStringListWhenObjectsHaveSameProperties()
    {
        #region Arrange

        var serviceProvider = new TestServiceProvider().CreateServices();

        var mapper = serviceProvider.GetRequiredService<IMapper>();

        var source = new StringCollectionPerson
        {
            Id = 1,
            Name = "Name",
            Surname = "LastName",
            Hobbies =
            [
                "hobby1", "hobby2"
            ]
        };

        #endregion

        #region Act

        var destination = mapper.Map<StringCollectionDestinationPerson>(source);

        #endregion

        #region Assert

        using (new AssertionScope())
        {
            destination.Id.Should().Be(source.Id);
            destination.Name.Should().Be(source.Name);
            destination.Surname.Should().Be(source.Surname);
            destination.Hobbies.Count.Should().Be(source.Hobbies.Count);
            destination.Hobbies.Count.Should().Be(source.Hobbies.Count);
            destination.Hobbies.First().Should().Be(source.Hobbies.First());
            destination.Hobbies.Last().Should().Be(source.Hobbies.Last());
        }

        #endregion
    }
}