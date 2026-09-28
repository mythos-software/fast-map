namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
///  Represents a mapping processor that can be used to customize the mapping process between source and destination types.
/// </summary>
public interface IMappingProcessor<TSource, TDestination>
{
    bool CanProcess(Type sourceType, Type destinationType) => sourceType == typeof(TSource) && destinationType == typeof(TDestination);
    
    TDestination Process(TSource source);
    
    TDestination Process(TSource source, TDestination destination);
}