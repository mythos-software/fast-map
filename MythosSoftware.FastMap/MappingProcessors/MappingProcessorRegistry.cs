namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents a registry for storing and retrieving mapping processors based on source and destination types.
/// </summary>
internal class MappingProcessorRegistry
{
    #region Fields
    
    private readonly List<object> _processors = new();
    
    #endregion
    
    #region Public Methods
    
    public void Register<TSource, TDestination>(IMappingProcessor<TSource, TDestination> processor)
    {
        _processors.Add(processor);
    }

    public void Register(object processor)
    {
        _processors.Add(processor);
    }

    public IEnumerable<object> GetAll()
    {
        return _processors;
    }

    public IEnumerable<IMappingProcessor<TSource, TDestination>> Get<TSource, TDestination>()
    {
        return _processors.OfType<IMappingProcessor<TSource, TDestination>>();
    }

    public IMappingProcessor<TSource, TDestination>? Find<TSource, TDestination>()
    {
        return _processors
            .OfType<IMappingProcessor<TSource, TDestination>>()
            .FirstOrDefault(p => p.CanProcess(typeof(TSource), typeof(TDestination)));
    }
    
    #endregion
}