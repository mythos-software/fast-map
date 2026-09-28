using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for testing enum mapping functionality in the FastMap library.
/// </summary>
public class EnumMappingTestsShould
{
    [Fact]
    public void MapSourceToDestinationWhenObjectIsEnum()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = UserType.Admin;
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<UserType>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Should().Be(source);
        }
        
        #endregion
    }

    [Fact]
    public void MapSourceToDestinationWhenObjectContainsEnum()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new User
        {
            Id = 1,
            Name = "John",
            Surname = "Doe",
            Type = UserType.Admin
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<User>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Type.Should().Be(source.Type);
        }
        
        #endregion
    }
    
    [Fact]
    public void MapSourceToDestinationWhenObjectContainsEnumAsString()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new StringUser
        {
            Id = 1,
            Name = "John",
            Surname = "Doe",
            Type = nameof(UserType.Admin)
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<User>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Type.Should().Be((UserType)Enum.Parse(typeof(UserType), source.Type));
        }
        
        #endregion
    }
    
    [Fact]
    public void MapSourceToDestinationWhenObjectContainsEnumToString()
    {
        #region Arrange
        
        var serviceProvider = new TestServiceProvider().CreateServices();
        
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var source = new User
        {
            Id = 1,
            Name = "John",
            Surname = "Doe",
            Type = UserType.Admin
        };
        
        #endregion
        
        #region Act
        
        var destination = mapper.Map<StringUser>(source);
        
        #endregion
        
        #region Assert

        using (new AssertionScope())
        {
            destination.Type.Should().Be(source.Type.ToString());
        }
        
        #endregion
    }
}