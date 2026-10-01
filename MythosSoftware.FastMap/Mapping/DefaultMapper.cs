using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

/// <summary>
/// Provides a default implementation of the IMapper interface.
/// </summary>
internal class DefaultMapper : IMapper
{
    #region Fileds
    
    private static int s_nextTypePairId = -1;

    private static readonly ConcurrentDictionary<(Type Source, Type Destination), Func<DefaultMapper, object, object?>> s_untypedMapDelegates = new();
    
    private readonly MappingProcessorRegistry _registry;

    private readonly object _processorSlotsLock = new();

    private object?[] _processorSlots = new object?[16];
    
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

        return ObjectMapDelegateCache<TDestination>.Get(source.GetType())(this, source);
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

        var mapDelegate = s_untypedMapDelegates.GetOrAdd((sourceType, destinationType), static key =>
            (Func<DefaultMapper, object, object?>)typeof(DefaultMapper)
                .GetMethod(nameof(MapUntyped), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(key.Source, key.Destination)
                .CreateDelegate(typeof(Func<DefaultMapper, object, object?>)));

        return mapDelegate(this, source);
    }
    
    #endregion
    
    #region Private Methods

    private static object? MapUntyped<TSource, TDestination>(DefaultMapper mapper, object source)
    {
        return mapper.GetProcessor<TSource, TDestination>().Process((TSource)source);
    }

    private static TDestination MapFromObject<TSource, TDestination>(DefaultMapper mapper, object source)
    {
        return mapper.GetProcessor<TSource, TDestination>().Process((TSource)source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IMappingProcessor<TSource, TDestination> GetProcessor<TSource, TDestination>()
    {
        var id = TypePairId<TSource, TDestination>.Value;
        var slots = _processorSlots;

        if ((uint)id < (uint)slots.Length && slots[id] is { } processor)
        {
            // Slots are only ever populated with the processor matching the type pair id.
            return Unsafe.As<IMappingProcessor<TSource, TDestination>>(processor);
        }

        return ResolveAndCacheProcessor<TSource, TDestination>(id);
    }

    private IMappingProcessor<TSource, TDestination> ResolveAndCacheProcessor<TSource, TDestination>(int id)
    {
        var processor = ResolveProcessor<TSource, TDestination>();

        lock (_processorSlotsLock)
        {
            var slots = _processorSlots;

            if (id >= slots.Length)
            {
                Array.Resize(ref slots, Math.Max(slots.Length * 2, id + 1));
            }
            else
            {
                slots = (object?[])slots.Clone();
            }

            slots[id] = processor;
            _processorSlots = slots;
        }

        return processor;
    }

    private IMappingProcessor<TSource, TDestination> ResolveProcessor<TSource, TDestination>()
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

    #region Nested Types

    private static class TypePairId<TSource, TDestination>
    {
        public static readonly int Value = Interlocked.Increment(ref s_nextTypePairId);
    }

    private static class ObjectMapDelegateCache<TDestination>
    {
        private static readonly ConcurrentDictionary<Type, Func<DefaultMapper, object, TDestination>> s_delegates = new();

        private static Entry? s_last;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<DefaultMapper, object, TDestination> Get(Type sourceType)
        {
            var last = s_last;

            if (last is not null && ReferenceEquals(last.SourceType, sourceType))
            {
                return last.Delegate;
            }

            var mapDelegate = s_delegates.GetOrAdd(sourceType, static type =>
                (Func<DefaultMapper, object, TDestination>)typeof(DefaultMapper)
                    .GetMethod(nameof(MapFromObject), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(type, typeof(TDestination))
                    .CreateDelegate(typeof(Func<DefaultMapper, object, TDestination>)));

            s_last = new Entry(sourceType, mapDelegate);

            return mapDelegate;
        }

        private sealed record Entry(Type SourceType, Func<DefaultMapper, object, TDestination> Delegate);
    }

    #endregion
}
