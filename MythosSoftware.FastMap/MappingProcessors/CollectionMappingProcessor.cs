using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Maps arrays, sequences, mutable collections, sets, queues, and dictionaries.
/// </summary>
internal sealed class CollectionMappingProcessor<TSource, TDestination>
    : IMappingProcessor<TSource, TDestination>
{
    private readonly ICollectionAdapter _adapter;

    public CollectionMappingProcessor(DefaultMapper mapper)
    {
        var sourceType = typeof(TSource);
        var destinationType = typeof(TDestination);
        var sourceDictionaryTypes = GetDictionaryTypes(sourceType);
        var destinationDictionaryTypes = GetDictionaryTypes(destinationType);

        if (sourceDictionaryTypes is not null && destinationDictionaryTypes is not null)
        {
            _adapter = (ICollectionAdapter)Activator.CreateInstance(
                typeof(DictionaryAdapter<,,,>).MakeGenericType(
                    typeof(TSource),
                    typeof(TDestination),
                    sourceDictionaryTypes.Value.Key,
                    sourceDictionaryTypes.Value.Value,
                    destinationDictionaryTypes.Value.Key,
                    destinationDictionaryTypes.Value.Value),
                mapper,
                destinationType)!;
            return;
        }

        var sourceElementType = GetElementType(sourceType)
            ?? throw new InvalidOperationException($"'{sourceType}' is not a supported collection.");
        var destinationElementType = GetElementType(destinationType)
            ?? throw new InvalidOperationException($"'{destinationType}' is not a supported collection.");

        _adapter = (ICollectionAdapter)Activator.CreateInstance(
            typeof(CollectionAdapter<,>).MakeGenericType(
                typeof(TSource),
                typeof(TDestination),
                sourceElementType,
                destinationElementType),
            mapper,
            destinationType)!;
    }

    public static bool CanHandle =>
        GetElementType(typeof(TSource)) is not null &&
        GetElementType(typeof(TDestination)) is not null;

    public TDestination Process(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return (TDestination)_adapter.Map(source);
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        return _adapter.TryMapTo(source, destination)
            ? destination
            : Process(source);
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

        return GetGenericInterface(type, typeof(IEnumerable<>))?.GetGenericArguments()[0];
    }

    private static (Type Key, Type Value)? GetDictionaryTypes(Type type)
    {
        var dictionaryType =
            GetGenericInterface(type, typeof(IDictionary<,>)) ??
            GetGenericInterface(type, typeof(IReadOnlyDictionary<,>));

        if (dictionaryType is null)
        {
            return null;
        }

        var arguments = dictionaryType.GetGenericArguments();
        return (arguments[0], arguments[1]);
    }

    private static Type? GetGenericInterface(Type type, Type genericTypeDefinition)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
        {
            return type;
        }

        return type.GetInterfaces()
            .FirstOrDefault(candidate =>
                candidate.IsGenericType &&
                candidate.GetGenericTypeDefinition() == genericTypeDefinition);
    }

    private interface ICollectionAdapter
    {
        object Map(object source);

        bool TryMapTo(object source, object destination);
    }

    private sealed class CollectionAdapter<TSourceElement, TDestinationElement>(
        DefaultMapper mapper,
        Type destinationType) : ICollectionAdapter
    {
        private readonly bool _mapElements =
            !typeof(TDestinationElement).IsAssignableFrom(typeof(TSourceElement)) ||
            mapper.HasRegisteredProcessor<TSourceElement, TDestinationElement>();

        private IMappingProcessor<TSourceElement, TDestinationElement>? _elementProcessor;

        public object Map(object source)
        {
            var items = MapItems((IEnumerable<TSourceElement>)source);

            if (destinationType.IsArray)
            {
                return items.ToArray();
            }

            if (destinationType.IsAssignableFrom(items.GetType()))
            {
                return items;
            }

            var set = new HashSet<TDestinationElement>(items);
            if (destinationType.IsAssignableFrom(set.GetType()))
            {
                return set;
            }

            if (CreateFromEnumerable(destinationType, items) is { } constructed)
            {
                return constructed;
            }

            if (!destinationType.IsAbstract && !destinationType.IsInterface)
            {
                var destination = Activator.CreateInstance(destinationType)
                    ?? throw UnsupportedCollection(destinationType);

                if (TryPopulate(destination, items))
                {
                    return destination;
                }
            }

            throw UnsupportedCollection(destinationType);
        }

        public bool TryMapTo(object source, object destination)
        {
            if (destination is Array)
            {
                return false;
            }

            return TryPopulate(destination, MapItems((IEnumerable<TSourceElement>)source));
        }

        private List<TDestinationElement> MapItems(IEnumerable<TSourceElement> source)
        {
            var capacity = source switch
            {
                ICollection<TSourceElement> collection => collection.Count,
                IReadOnlyCollection<TSourceElement> collection => collection.Count,
                _ => 0
            };
            var result = new List<TDestinationElement>(capacity);

            foreach (var item in source)
            {
                if (item is null)
                {
                    result.Add(default!);
                }
                else if (_mapElements)
                {
                    result.Add(MapElement(item));
                }
                else
                {
                    result.Add((TDestinationElement)(object)item);
                }
            }

            return result;
        }

        private TDestinationElement MapElement(TSourceElement item)
        {
            if (typeof(TSourceElement).IsValueType || item.GetType() == typeof(TSourceElement))
            {
                _elementProcessor ??= mapper.GetMappingProcessor<TSourceElement, TDestinationElement>();
                return _elementProcessor.Process(item);
            }

            return (TDestinationElement)mapper.Map(
                item,
                item.GetType(),
                typeof(TDestinationElement))!;
        }

        private static bool TryPopulate(object destination, IEnumerable<TDestinationElement> items)
        {
            if (destination is ICollection<TDestinationElement> collection)
            {
                if (collection.IsReadOnly)
                {
                    return false;
                }

                foreach (var item in items)
                {
                    collection.Add(item);
                }

                return true;
            }

            if (destination is IList list)
            {
                if (list.IsFixedSize || list.IsReadOnly)
                {
                    return false;
                }

                foreach (var item in items)
                {
                    list.Add(item);
                }

                return true;
            }

            var insert = CreateInserter(destination.GetType());
            if (insert is null)
            {
                return false;
            }

            foreach (var item in items)
            {
                insert(destination, item);
            }

            return true;
        }

        private static Action<object, TDestinationElement>? CreateInserter(Type type)
        {
            var method = new[] { "Add", "Enqueue", "Push" }
                .Select(name => type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, [typeof(TDestinationElement)]))
                .FirstOrDefault(candidate => candidate is not null);

            if (method is null)
            {
                return null;
            }

            var destination = Expression.Parameter(typeof(object), "destination");
            var item = Expression.Parameter(typeof(TDestinationElement), "item");
            var call = Expression.Call(Expression.Convert(destination, type), method, item);
            Expression body = method.ReturnType == typeof(void)
                ? call
                : Expression.Block(call, Expression.Empty());

            return Expression.Lambda<Action<object, TDestinationElement>>(body, destination, item).Compile();
        }
    }

    private sealed class DictionaryAdapter<TSourceKey, TSourceValue, TDestinationKey, TDestinationValue>(
        DefaultMapper mapper,
        Type destinationType) : ICollectionAdapter
        where TDestinationKey : notnull
    {
        private readonly bool _mapKeys =
            !typeof(TDestinationKey).IsAssignableFrom(typeof(TSourceKey)) ||
            mapper.HasRegisteredProcessor<TSourceKey, TDestinationKey>();

        private readonly bool _mapValues =
            !typeof(TDestinationValue).IsAssignableFrom(typeof(TSourceValue)) ||
            mapper.HasRegisteredProcessor<TSourceValue, TDestinationValue>();

        private IMappingProcessor<TSourceKey, TDestinationKey>? _keyProcessor;

        private IMappingProcessor<TSourceValue, TDestinationValue>? _valueProcessor;

        public object Map(object source)
        {
            var items = MapItems((IEnumerable<KeyValuePair<TSourceKey, TSourceValue>>)source);

            if (destinationType.IsAssignableFrom(items.GetType()))
            {
                return items;
            }

            if (CreateFromEnumerable(destinationType, items) is { } constructed)
            {
                return constructed;
            }

            if (!destinationType.IsAbstract && !destinationType.IsInterface)
            {
                var destination = Activator.CreateInstance(destinationType)
                    ?? throw UnsupportedCollection(destinationType);

                if (TryPopulate(destination, items))
                {
                    return destination;
                }
            }

            throw UnsupportedCollection(destinationType);
        }

        public bool TryMapTo(object source, object destination)
        {
            return TryPopulate(
                destination,
                MapItems((IEnumerable<KeyValuePair<TSourceKey, TSourceValue>>)source));
        }

        private Dictionary<TDestinationKey, TDestinationValue> MapItems(
            IEnumerable<KeyValuePair<TSourceKey, TSourceValue>> source)
        {
            var capacity = source switch
            {
                ICollection<KeyValuePair<TSourceKey, TSourceValue>> collection => collection.Count,
                IReadOnlyCollection<KeyValuePair<TSourceKey, TSourceValue>> collection => collection.Count,
                _ => 0
            };
            var result = new Dictionary<TDestinationKey, TDestinationValue>(capacity);

            foreach (var pair in source)
            {
                var key = MapKey(pair.Key);
                var value = MapValue(pair.Value);
                result.Add(key, value);
            }

            return result;
        }

        private TDestinationKey MapKey(TSourceKey key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (!_mapKeys)
            {
                return (TDestinationKey)(object)key;
            }

            if (typeof(TSourceKey).IsValueType || key.GetType() == typeof(TSourceKey))
            {
                _keyProcessor ??= mapper.GetMappingProcessor<TSourceKey, TDestinationKey>();
                return _keyProcessor.Process(key);
            }

            return (TDestinationKey)mapper.Map(key, key.GetType(), typeof(TDestinationKey))!;
        }

        private TDestinationValue MapValue(TSourceValue value)
        {
            if (value is null)
            {
                return default!;
            }

            if (!_mapValues)
            {
                return (TDestinationValue)(object)value;
            }

            if (typeof(TSourceValue).IsValueType || value.GetType() == typeof(TSourceValue))
            {
                _valueProcessor ??= mapper.GetMappingProcessor<TSourceValue, TDestinationValue>();
                return _valueProcessor.Process(value);
            }

            return (TDestinationValue)mapper.Map(value, value.GetType(), typeof(TDestinationValue))!;
        }

        private static bool TryPopulate(
            object destination,
            IEnumerable<KeyValuePair<TDestinationKey, TDestinationValue>> items)
        {
            if (destination is not IDictionary<TDestinationKey, TDestinationValue> dictionary ||
                dictionary.IsReadOnly)
            {
                return false;
            }

            foreach (var pair in items)
            {
                dictionary.Add(pair.Key, pair.Value);
            }

            return true;
        }
    }

    private static object? CreateFromEnumerable(Type destinationType, object items)
    {
        var constructor = destinationType
            .GetConstructors()
            .Where(candidate => candidate.GetParameters().Length == 1)
            .Select(candidate => (Constructor: candidate, Parameter: candidate.GetParameters()[0].ParameterType))
            .Where(candidate =>
                candidate.Parameter != typeof(object) &&
                candidate.Parameter.IsAssignableFrom(items.GetType()))
            .OrderByDescending(candidate => candidate.Parameter == items.GetType())
            .Select(candidate => candidate.Constructor)
            .FirstOrDefault();

        return constructor?.Invoke([items]);
    }

    private static InvalidOperationException UnsupportedCollection(Type destinationType)
    {
        return new InvalidOperationException($"Collection type '{destinationType}' is not supported.");
    }
}
