namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents a builder for creating instances of IMappingProcessor.
/// </summary>
internal static class MappingProcessorBuilder<TSource, TDestination>
{
    private static readonly List<IMappingProcessor<TSource, TDestination>> Processors = new();
    
    public static void Register(IMappingProcessor<TSource, TDestination> processor)
    {
        Processors.Add(processor);
    }
    
    public static IMappingProcessor<TSource, TDestination> Build()
    {
        var processor = Processors.FirstOrDefault(p => p.CanProcess(typeof(TSource), typeof(TDestination)));
        
        if (processor is not null)
        {
            return processor;
        }
        
        if (CollectionMappingProcessor<TSource, TDestination>.CanHandle)
        {
            return new CollectionMappingProcessor<TSource, TDestination>();
        }
        
        throw new InvalidOperationException($"No mapping configuration exists for {typeof(TSource).FullName} -> {typeof(TDestination).FullName}");
    }
}