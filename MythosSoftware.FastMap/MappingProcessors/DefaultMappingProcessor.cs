using System.Linq.Expressions;
using System.Reflection;

namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents the default implementation of the IMappingProcessor interface, providing basic mapping functionality between source and destination types.
/// Will be used if no custom mapping processor is provided. It can be extended or replaced with a custom implementation to handle specific mapping scenarios.
/// </summary>
internal class DefaultMappingProcessor<TSource, TDestination> : IMappingProcessor<TSource, TDestination>
{
    #region Fields
    
    private readonly Dictionary<string, MemberMapping> _memberMappings = new();
    
    #endregion
    
    #region IMappingProcessor
    
    public TDestination Process(TSource source)
    {
        var destination = Activator.CreateInstance<TDestination>();

        return Process(source, destination);
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        MapDefaultProperties(source, destination);
        MapConfiguredProperties(source, destination);

        return destination;
    }
    
    #endregion
    
    #region Internal Methods
    
    internal void AddMemberMapping<TMember>(
        Expression<Func<TDestination, TMember>> destinationExpression,
        MemberConfigurationExpression<TSource, TDestination, TMember> configuration)
    {
        var destinationProperty = GetProperty(destinationExpression);

        _memberMappings[destinationProperty.Name] = new MemberMapping
        {
            DestinationProperty = destinationProperty,
            Ignored = configuration.IsIgnored,
            SourceGetter = configuration.SourceExpression?.Compile()
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

            // ForMember controls this destination property.
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

            // Complex property: must go through a mapping processor.
            var mappedValue = MappingProcessorInvoker.Map(sourceValue, sourceType, destinationPropertyType);

            destinationProperty.SetValue(destination, mappedValue);
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