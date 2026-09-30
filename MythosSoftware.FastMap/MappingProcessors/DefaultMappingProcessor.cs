using System.Linq.Expressions;
using System.Reflection;

namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents the default implementation of the IMappingProcessor interface, providing basic mapping functionality between source and destination types.
/// Will be used if no custom mapping processor is provided. It can be extended or replaced with a custom implementation to handle specific mapping scenarios.
/// </summary>
internal class DefaultMappingProcessor<TSource, TDestination>(Profile? profile = null)
    : IMappingProcessor<TSource, TDestination>, IRequireMapper
{
    #region Fields
    
    private readonly Dictionary<string, MemberMapping> _memberMappings = new();
    
    #endregion

    #region Properties
    
    internal Profile? Profile { get; } = profile;

    internal DefaultMapper? Mapper { get; private set; }

    public Dictionary<string, MemberMapping> MemberMappings => _memberMappings;
    
    #endregion

    #region IRequireMapper

    public void SetMapper(DefaultMapper mapper)
    {
        Mapper = mapper;
    }

    #endregion
    
    #region IMappingProcessor
    
    public TDestination Process(TSource source)
    {
        var destination = Activator.CreateInstance<TDestination>();

        return Process(source, destination);
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        if (destination != null && source is TDestination && IsSimpleType(destination.GetType()))
        {
            return (TDestination)(object)source;
        }
        
        MapDefaultProperties(source, destination);
        MapConfiguredProperties(source, destination);

        return destination;
    }
    
    #endregion
    
    #region Internal Methods
    
    internal void AddMemberMapping(Dictionary<string, MemberMapping> memberMappings)
    {
        _memberMappings.Clear();
        foreach (var kvp in memberMappings)
        {
            _memberMappings[kvp.Key] = kvp.Value;
        }
    }
    
    internal void AddMemberMapping<TMember>(
        Expression<Func<TDestination, TMember>> destinationExpression,
        MemberConfigurationExpression<TSource, TDestination, TMember> configuration)
    {
        var destinationProperty = GetProperty(destinationExpression);

        _memberMappings[destinationProperty.Name] = new MemberMapping
        {
            DestinationProperty = destinationProperty,
            SourceProperty = configuration.SourceExpression is null ? null : GetPropertyInfo(configuration.SourceExpression),
            Ignored = configuration.IsIgnored,
            SourceGetter = configuration.SourceExpression?.Compile()
        };
    }
    
    internal MemberMapping? GetReversedMemberMapping(
        MemberMapping originalMapping)
    {
        if (originalMapping.Ignored)
            return null;

        if (originalMapping.SourceProperty is null)
        {
            throw new InvalidOperationException(
                $"Mapping to '{originalMapping.DestinationProperty.Name}' " +
                "cannot be reversed because its source is not a direct property.");
        }

        var reverseDestinationProperty = originalMapping.SourceProperty;
        var reverseSourceProperty = originalMapping.DestinationProperty;

        var sourceParameter = Expression.Parameter(
            typeof(TDestination),
            "src");

        var sourcePropertyExpression = Expression.Property(
            sourceParameter,
            reverseSourceProperty);

        var delegateType = typeof(Func<,>).MakeGenericType(
            typeof(TDestination),
            reverseSourceProperty.PropertyType);

        var sourceGetter = Expression.Lambda(
                delegateType,
                sourcePropertyExpression,
                sourceParameter)
            .Compile();

        return new MemberMapping
        {
            DestinationProperty = reverseDestinationProperty,
            SourceProperty = reverseSourceProperty,
            SourceGetter = sourceGetter,
            Ignored = false
        };
    }
    
    #endregion
    
    #region Private Methods
    
    private void MapDefaultProperties(TSource source, TDestination destination)
    {
        var sourceProperties = typeof(TSource).GetProperties();
        var destinationType = typeof(TDestination);

        foreach (var sourceProperty in sourceProperties)
        {
            var destinationProperty = destinationType.GetProperty(sourceProperty.Name);

            if (destinationProperty is null || !destinationProperty.CanWrite)
            {
                continue;
            }
            
            if (_memberMappings.ContainsKey(destinationProperty.Name))
            {
                continue;
            }
            
            var sourceValue = sourceProperty.GetValue(source);

            if (sourceValue is null)
            {
                destinationProperty.SetValue(destination, null);
                continue;
            }
            
            var sourceType = sourceProperty.PropertyType;
            var destinationPropertyType = destinationProperty.PropertyType;

            if (IsSimpleType(sourceType) && IsSimpleType(destinationPropertyType))
            {
                destinationProperty.SetValue(destination, sourceValue);
                continue;
            }
            
            if (Mapper is not null)
            {
                var mappedValue = Mapper.Map(sourceValue, sourceType, destinationPropertyType);
                destinationProperty.SetValue(destination, mappedValue);
            }
        }
    }

    private void MapConfiguredProperties(TSource source, TDestination destination)
    {
        foreach (var mapping in _memberMappings.Values)
        {
            if (mapping.Ignored)
            {
                continue;
            }

            if (mapping.SourceGetter is null)
            {
                continue;
            }

            var value = mapping.SourceGetter.DynamicInvoke(source);

            mapping.DestinationProperty.SetValue(destination, value);
        }
    }
    
    private static PropertyInfo GetProperty<TMember>(Expression<Func<TDestination, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo property)
        {
            return property;
        }

        throw new ArgumentException("Expression must reference a destination property.", nameof(expression));
    }
    
    private static PropertyInfo? GetPropertyInfo(LambdaExpression expression)
    {
        Expression body = expression.Body;
        
        if (body is UnaryExpression unary &&
            unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression memberExpression
               && memberExpression.Member is PropertyInfo property
            ? property
            : null;
    }
    
    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type == typeof(string)
               || type.IsPrimitive
               || type == typeof(bool)
               || type == typeof(byte)
               || type == typeof(sbyte)
               || type == typeof(short)
               || type == typeof(ushort)
               || type == typeof(int)
               || type == typeof(uint)
               || type == typeof(long)
               || type == typeof(ulong)
               || type == typeof(float)
               || type == typeof(double)
               || type == typeof(decimal)
               || type == typeof(char)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid)
               || type.IsEnum;
    }
    
    #endregion
}