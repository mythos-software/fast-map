using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap.Tests.Objects;
using MythosSoftware.FastMap.Tests.Providers;

namespace MythosSoftware.FastMap.Tests;

/// <summary>
/// Test class for regression testing
/// </summary>
public class CollectionMappingRegressionTestsShould
{
    [Fact]
    public void MapListToArray()
    {
        var mapper = CreateMapper();
        var source = new List<SimplePerson>
        {
            new() { Id = 1, Name = "Jane", Surname = "Doe" }
        };

        var destination = mapper.Map<SimpleDestinationPerson[]>(source);

        destination.Should().ContainSingle();
        destination[0].Name.Should().Be("Jane");
    }

    [Fact]
    public void MapListToCollectionInterface()
    {
        var mapper = CreateMapper();

        var destination = mapper.Map<IReadOnlyList<int>>(new[] { 1, 2, 3 });

        destination.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void MapListToSet()
    {
        var mapper = CreateMapper();

        var destination = mapper.Map<ISet<int>>(new[] { 1, 1, 2 });

        destination.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void MapListToQueue()
    {
        var mapper = CreateMapper();

        var destination = mapper.Map<Queue<int>>(new[] { 1, 2, 3 });

        destination.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void MapDictionaryValues()
    {
        var mapper = CreateMapper();
        var source = new Dictionary<int, SimplePerson>
        {
            [7] = new() { Id = 1, Name = "Jane", Surname = "Doe" }
        };

        var destination = mapper.Map<IReadOnlyDictionary<int, ModifiedSimplePerson>>(source);

        destination.Should().ContainKey(7);
        destination[7].FirstName.Should().Be("Jane");
        destination[7].LastName.Should().Be("Doe");
    }

    [Fact]
    public void AppendToExistingMutableCollection()
    {
        var mapper = CreateMapper();
        ICollection<int> destination = new HashSet<int> { 1 };

        var result = mapper.Map(new[] { 2, 3 }, destination);

        result.Should().BeSameAs(destination);
        destination.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void ReplaceExistingArrayInsteadOfAddingToFixedSizeList()
    {
        var mapper = CreateMapper();
        var destination = new[] { 9 };

        var result = mapper.Map(new[] { 1, 2 }, destination);

        result.Should().Equal(1, 2);
        result.Should().NotBeSameAs(destination);
    }

    [Fact]
    public void CopyUnconfiguredIdentityElements()
    {
        var mapper = CreateMapper();
        var item = new UnconfiguredCollectionItem { Value = 42 };

        var destination = mapper.Map<List<UnconfiguredCollectionItem>>(new[] { item });

        destination.Should().ContainSingle().Which.Should().BeSameAs(item);
    }

    [Fact]
    public void MapPolymorphicElementsUsingTheirRuntimeType()
    {
        var mapper = CreateMapper();
        object[] source = ["first", "second"];

        var destination = mapper.Map<List<string>>(source);

        destination.Should().Equal("first", "second");
    }

    private static IMapper CreateMapper()
    {
        return new TestServiceProvider().CreateServices().GetRequiredService<IMapper>();
    }

    private sealed class UnconfiguredCollectionItem
    {
        public int Value { get; init; }
    }
}
