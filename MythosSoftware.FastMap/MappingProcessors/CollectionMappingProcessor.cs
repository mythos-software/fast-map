using System.Collections;
using System.Reflection;

namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents a mapping processor that handles the mapping of collection types (e.g., arrays, lists) from a source type to a destination type.
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TDestination"></typeparam>
internal sealed class CollectionMappingProcessor<TSource, TDestination> : IMappingProcessor<TSource, TDestination>
{
    public static bool CanHandle => GetElementType(typeof(TSource)) is not null && GetElementType(typeof(TDestination)) is not null;

    public TDestination Process(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var destinationElementType =
            GetElementType(typeof(TDestination))
            ?? throw new InvalidOperationException(
                $"{typeof(TDestination)} is not a supported collection.");

        var mappedItems = MapItems((IEnumerable)(object)source, destinationElementType);

        return CreateDestination(mappedItems, destinationElementType);
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var destinationElementType =
            GetElementType(typeof(TDestination))
            ?? throw new InvalidOperationException(
                $"{typeof(TDestination)} is not a supported collection.");

        /*
         * Existing mutable collection.
         */
        if (destination is IList destinationList)
        {
            foreach (var sourceItem in (IEnumerable)(object)source)
            {
                if (sourceItem is null)
                {
                    destinationList.Add(null);
                    continue;
                }

                destinationList.Add(MapItem(sourceItem, destinationElementType));
            }

            return destination;
        }

        /*
         * Destination cannot be modified directly,
         * so create a new collection.
         */
        return Process(source);
    }

    private static IList MapItems(IEnumerable source, Type destinationElementType)
    {
        var listType = typeof(List<>).MakeGenericType(destinationElementType);

        var result = (IList)Activator.CreateInstance(listType)!;

        foreach (var item in source)
        {
            if (item is null)
            {
                result.Add(null);
                continue;
            }

            result.Add(MapItem(item, destinationElementType));
        }

        return result;
    }

    private static object MapItem(object source, Type destinationType)
    {
        var method = typeof(CollectionMappingProcessor<TSource, TDestination>)
            .GetMethod(
                nameof(MapItemGeneric),
                BindingFlags.NonPublic |
                BindingFlags.Static)!
            .MakeGenericMethod(
                source.GetType(),
                destinationType);

        return method.Invoke(null, new[] { source })!;
    }

    private static object MapItemGeneric<TItemSource, TItemDestination>(object source)
    {
        var processor = MappingProcessorBuilder<TItemSource, TItemDestination>.Build();

        return processor.Process((TItemSource)source)!;
    }

    private static TDestination CreateDestination(IList items, Type elementType)
    {
        var destinationType = typeof(TDestination);

        /*
         * Array
         */
        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(elementType, items.Count);

            items.CopyTo(array, 0);

            return (TDestination)(object)array;
        }

        /*
         * List<T> can also satisfy:
         *
         * IEnumerable<T>
         * ICollection<T>
         * IList<T>
         * IReadOnlyCollection<T>
         * IReadOnlyList<T>
         */
        if (destinationType.IsAssignableFrom(items.GetType()))
        {
            return (TDestination)items;
        }

        /*
         * Concrete collection.
         */
        var destination = Activator.CreateInstance(destinationType);

        if (destination is IList destinationList)
        {
            foreach (var item in items)
            {
                destinationList.Add(item);
            }

            return (TDestination)destination;
        }

        throw new InvalidOperationException($"Collection type '{destinationType}' is not supported.");
    }

    private static Type? GetElementType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() ==
            typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        var enumerableInterface =
            type.GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() ==
                    typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }
}