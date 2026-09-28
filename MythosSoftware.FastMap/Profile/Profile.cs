using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

/// <summary>
/// Represents a profile that defines mappings between source and destination types.
/// </summary>
public class Profile
{
    #region Fields
    
    private readonly List<object> _processors = new();

    internal IReadOnlyList<object> Processors => _processors;
    
    #endregion

    #region Methods

    protected IMappingProcessor<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var processor = new DefaultMappingProcessor<TSource, TDestination>(this);

        _processors.Add(processor);

        return processor;
    }

    internal void RegisterProcessor(object processor)
    {
        _processors.Add(processor);
    }
    
    #endregion
}