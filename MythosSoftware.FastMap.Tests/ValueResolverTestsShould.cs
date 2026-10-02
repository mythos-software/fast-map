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

    class CurrentValueResolver : IValueResolver<SimplePerson, SimplePerson, string>
    {
        public string Resolve(SimplePerson source, SimplePerson destination, string currentValue)
        {
            return $"{source.Surname} {destination.Name} {currentValue}";
        }
    }

    class IdResolver : IValueResolver<SimplePerson, SimplePerson, int>
    {
        public int Resolve(SimplePerson source, SimplePerson destination, int currentValue)
        {
            return source.Id + currentValue;
        }
    }

    class NullResolver : IValueResolver<SimplePerson, SimplePerson, string?>
    {
        public string? Resolve(SimplePerson source, SimplePerson destination, string? currentValue)
        {
            return null;
        }
    }

    class ThrowingResolver : IValueResolver<SimplePerson, SimplePerson, string>
    {
        public string Resolve(SimplePerson source, SimplePerson destination, string currentValue)
        {
            throw new InvalidOperationException("Resolver failed.");
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

    class ConfigurableResolverTestProfile : Profile
    {
        public IMappingProcessor<SimplePerson, SimplePerson> Mapping { get; }

        public ConfigurableResolverTestProfile()
        {
            Mapping = CreateMap<SimplePerson, SimplePerson>();
        }
    }

    #endregion

    [Fact]
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

    [Fact]
    public void PassDestinationAndCurrentMemberValueToResolver()
    {
        var profile = new ConfigurableResolverTestProfile();
        profile.Mapping.ForMember(dest => dest.Surname, opt => opt.MapFrom<CurrentValueResolver>());
        using var serviceProvider = new ServiceCollection().AddFastMap(profile).BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var source = new SimplePerson { Id = 1, Name = "NewName", Surname = "NewSurname" };
        var destination = new SimplePerson { Id = 2, Name = "OldName", Surname = "OldSurname" };

        var result = mapper.Map(source, destination);

        result.Should().BeSameAs(destination);
        result.Surname.Should().Be("NewSurname NewName OldSurname");
        result.Id.Should().Be(source.Id);
    }

    [Fact]
    public void MapValueTypeMemberUsingResolver()
    {
        var profile = new ConfigurableResolverTestProfile();
        profile.Mapping.ForMember(dest => dest.Id, opt => opt.MapFrom<IdResolver>());
        var source = new SimplePerson { Id = 3 };
        var destination = new SimplePerson { Id = 7 };

        profile.Mapping.Process(source, destination).Id.Should().Be(10);
        profile.Mapping.Process(source).Id.Should().Be(3);
    }

    [Fact]
    public void AssignNullReturnedByResolver()
    {
        var profile = new ConfigurableResolverTestProfile();
        profile.Mapping.ForMember<SimplePerson, SimplePerson, string?>(
            dest => dest.Surname, opt => opt.MapFrom<NullResolver>());

        var result = profile.Mapping.Process(
            new SimplePerson { Surname = "SourceSurname" },
            new SimplePerson { Surname = "ExistingSurname" });

        result.Surname.Should().BeNull();
    }

    [Fact]
    public void RebuildCompiledMappingWhenResolverConfigurationChanges()
    {
        var profile = new ConfigurableResolverTestProfile();
        var source = new SimplePerson { Id = 1, Name = "Name", Surname = "Surname" };
        profile.Mapping.ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Name));
        profile.Mapping.Process(source).Surname.Should().Be("Name");

        var mapping = profile.Mapping.ForMember(dest => dest.Surname, opt => opt.MapFrom<CustomResolver>());

        mapping.Should().BeSameAs(profile.Mapping);
        profile.Mapping.Process(source).Surname.Should().Be("1 Name Surname");

        profile.Mapping.ForMember(dest => dest.Surname, opt => opt.Ignore());
        profile.Mapping.Process(source, new SimplePerson { Surname = "Existing" }).Surname.Should().Be("Existing");
    }

    [Fact]
    public void PropagateResolverException()
    {
        var profile = new ConfigurableResolverTestProfile();
        profile.Mapping.ForMember(dest => dest.Surname, opt => opt.MapFrom<ThrowingResolver>());

        Action act = () => profile.Mapping.Process(new SimplePerson());

        act.Should().Throw<InvalidOperationException>().WithMessage("Resolver failed.");
    }

    [Fact]
    public void RejectReversingResolverMapping()
    {
        var profile = new ConfigurableResolverTestProfile();
        profile.Mapping.ForMember(dest => dest.Surname, opt => opt.MapFrom<CustomResolver>());

        Action act = () => profile.Mapping.ReverseMap();

        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot be reversed*");
    }

    [Fact]
    public void RejectMissingResolverConfiguration()
    {
        var profile = new ConfigurableResolverTestProfile();

        Action act = () => profile.Mapping.ForMember(
            dest => dest.Surname, (MemberOptions<SimplePerson, SimplePerson, string> _) => { });

        act.Should().Throw<InvalidOperationException>().WithMessage("*MapFrom<TResolver>()*");
    }

    [Fact]
    public void RejectNonPropertyDestinationExpression()
    {
        var profile = new ConfigurableResolverTestProfile();

        Action act = () => profile.Mapping.ForMember(dest => dest.Surname.ToUpper(), opt => opt.MapFrom<CustomResolver>());

        act.Should().Throw<ArgumentException>().WithMessage("*destination property*");
    }

    [Fact]
    public void RejectNestedDestinationProperty()
    {
        var profile = new ConfigurableResolverTestProfile();

        Action act = () => profile.Mapping.ForMember(dest => dest.Name.Length, opt => opt.MapFrom<IdResolver>());

        act.Should().Throw<ArgumentException>().WithMessage("*directly on the destination*");
    }

    [Fact]
    public void RejectNullArguments()
    {
        var profile = new ConfigurableResolverTestProfile();
        IMappingProcessor<SimplePerson, SimplePerson> mapping = null!;
        Action<MemberOptions<SimplePerson, SimplePerson, string>> configure = opt => opt.MapFrom<CustomResolver>();

        Action nullMapping = () => mapping.ForMember(dest => dest.Surname, configure);
        Action nullMember = () => profile.Mapping.ForMember(null!, configure);
        Action nullConfigure = () => profile.Mapping.ForMember(dest => dest.Surname,
            (Action<MemberOptions<SimplePerson, SimplePerson, string>>)null!);

        nullMapping.Should().Throw<ArgumentNullException>().WithParameterName("mapping");
        nullMember.Should().Throw<ArgumentNullException>().WithParameterName("destinationMember");
        nullConfigure.Should().Throw<ArgumentNullException>().WithParameterName("configure");
    }
}
