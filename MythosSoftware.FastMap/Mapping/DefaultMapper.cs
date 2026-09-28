using System.Reflection;
using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

/// <summary>
/// Provides a default implementation of the IMapper interface.
/// </summary>
internal class DefaultMapper : IMapper
{
    #region Fileds
    
    private readonly MappingProcessorRegistry _registry;
    
    #endregion
    
    #region Constructors

    public DefaultMapper(MappingProcessorRegistry registry)
    {
        _registry = registry;

        foreach (var processor in _registry.GetAll())
        {
            if (processor is IRequireMapper requireMapper)
            {
                requireMapper.SetMapper(this);
            }
        }
    }

    #endregion

    #region IMapper

    public TDestination Map<TDestination>(object source)
    {
        if (source is null)
        {
            return default!;
        }

        return MappingInvoker<TDestination>.Invoke(this, (dynamic)source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetProcessor<TSource, TDestination>();
        
        return mapper.Process(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = GetProcessor<TSource, TDestination>();
        
        return mapper.Process(source, destination);
    }

    #endregion

    #region Public Methods
    
    public object? Map(object? source, Type sourceType, Type destinationType)
    {
        if (source is null)
        {
            return null;
        }

        var method = typeof(DefaultMapper)
            .GetMethod(nameof(MapGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(sourceType, destinationType);

        return method.Invoke(this, new[] { source });
    }
    
    #endregion
    
    #region Private Methods

    private object? MapGeneric<TSource, TDestination>(object source)
    {
        var processor = GetProcessor<TSource, TDestination>();
        return processor.Process((TSource)source);
    }

    private IMappingProcessor<TSource, TDestination> GetProcessor<TSource, TDestination>()
    {
        var processor = _registry.Find<TSource, TDestination>();
        
        if (processor is not null)
        {
            if (processor is IRequireMapper requireMapper)
            {
                requireMapper.SetMapper(this);
            }
            return processor;
        }
        
        if (CollectionMappingProcessor<TSource, TDestination>.CanHandle)
        {
            return new CollectionMappingProcessor<TSource, TDestination>(this);
        }
        
        throw new InvalidOperationException($"No mapping configuration exists for {typeof(TSource).FullName} -> {typeof(TDestination).FullName}");
    }
    
    #endregion
}