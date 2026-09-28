using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

/// <summary>
/// Provides a default implementation of the IMapper interface.
/// </summary>
internal class DefaultMapper : IMapper
{
    #region IMapper

    public TDestination Map<TDestination>(object source)
    {
        return MappingInvoker<TDestination>.Invoke(this, (dynamic)source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = MappingProcessorBuilder<TSource, TDestination>.Build();
        
        return mapper.Process(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = MappingProcessorBuilder<TSource, TDestination>.Build();
        
        return mapper.Process(source, destination);
    }

    #endregion
}