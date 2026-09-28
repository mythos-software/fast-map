namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents a mapping processor that handles the mapping of simple types (e.g., primitive types, strings) from a source type to a destination type.
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TDestination"></typeparam>
internal class SimpleTypeMappingProcessor<TSource, TDestination> : IMappingProcessor<TSource, TDestination>
{
    public TDestination Process(TSource source)
    {
        if (source is TDestination destination)
            return destination;

        return (TDestination)Convert.ChangeType(source!, typeof(TDestination));
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        return Process(source);
    }
}