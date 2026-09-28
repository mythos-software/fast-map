using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

/// <summary>
/// Represents a profile that defines mappings between source and destination types.
/// </summary>
public class Profile
{
    protected IMappingProcessor<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var processor = new DefaultMappingProcessor<TSource, TDestination>();

        MappingProcessorBuilder<TSource, TDestination>.Register(processor);

        return processor;
    }
}