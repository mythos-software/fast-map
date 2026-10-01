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
    
    private static readonly Func<TDestination> s_createDestination = CreateDestinationFactory();

    private readonly Dictionary<string, MemberMapping> _memberMappings = new();

    private Func<TSource, TDestination, TDestination>? _mapDelegate;
    
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
        return Process(source, s_createDestination());
    }

    public TDestination Process(TSource source, TDestination destination)
    {
        if (destination != null && source is TDestination && IsSimpleType(destination.GetType()))
        {
            return (TDestination)(object)source;
        }
        
        return (_mapDelegate ??= BuildMapDelegate())(source, destination);
    }
    
    #endregion
    
    #region Internal Methods
    
    internal void AddMemberMapping(Dictionary<string, MemberMapping> memberMappings)
    {
        _mapDelegate = null;
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
        _mapDelegate = null;

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
    
    private Func<TSource, TDestination, TDestination> BuildMapDelegate()
    {
        var source = Expression.Parameter(typeof(TSource), "source");
        var destination = Expression.Parameter(typeof(TDestination), "destination");
        var body = new List<Expression>();

        AddDefaultPropertyAssignments(source, destination, body);
        AddConfiguredPropertyAssignments(source, destination, body);

        body.Add(destination);

        return Expression
            .Lambda<Func<TSource, TDestination, TDestination>>(Expression.Block(body), source, destination)
            .Compile();
    }

    private void AddDefaultPropertyAssignments(ParameterExpression source, ParameterExpression destination, List<Expression> body)
    {
        var destinationType = typeof(TDestination);

        foreach (var sourceProperty in typeof(TSource).GetProperties())
        {
            var getter = sourceProperty.GetGetMethod(true);

            if (getter is null || getter.IsStatic || sourceProperty.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var destinationProperty = destinationType.GetProperty(sourceProperty.Name);

            if (destinationProperty is null || !destinationProperty.CanWrite)
            {
                continue;
            }

            var setter = destinationProperty.GetSetMethod(true);

            if (setter is null || setter.IsStatic || destinationProperty.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (_memberMappings.ContainsKey(destinationProperty.Name))
            {
                continue;
            }

            body.Add(BuildDefaultAssignment(source, destination, sourceProperty, destinationProperty));
        }
    }

    private Expression BuildDefaultAssignment(
        ParameterExpression source,
        ParameterExpression destination,
        PropertyInfo sourceProperty,
        PropertyInfo destinationProperty)
    {
        var sourceType = sourceProperty.PropertyType;
        var destinationPropertyType = destinationProperty.PropertyType;
        var value = Expression.Variable(sourceType, "value");
        var destinationMember = Expression.Property(destination, destinationProperty);

        Expression assignNonNull;

        if (IsSimpleType(sourceType) && IsSimpleType(destinationPropertyType))
        {
            assignNonNull = BuildSimpleAssignment(destination, destinationProperty, value);
        }
        else
        {
            // Nested mapping is resolved through the mapper at runtime, matching the previous behaviour
            // where nothing is assigned when no mapper is attached.
            var mapper = Expression.Property(Expression.Constant(this), nameof(Mapper));
            var mapMethod = typeof(DefaultMapper)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == nameof(DefaultMapper.Map)
                             && m.IsGenericMethodDefinition
                             && m.GetGenericArguments().Length == 2
                             && m.GetParameters().Length == 1)
                .MakeGenericMethod(sourceType, destinationPropertyType);

            assignNonNull = Expression.IfThen(
                Expression.NotEqual(mapper, Expression.Constant(null, typeof(DefaultMapper))),
                Expression.Assign(destinationMember, Expression.Call(mapper, mapMethod, value)));
        }

        Expression assignment = assignNonNull;

        if (!sourceType.IsValueType || Nullable.GetUnderlyingType(sourceType) is not null)
        {
            assignment = Expression.IfThenElse(
                Expression.Equal(value, Expression.Constant(null, sourceType)),
                Expression.Assign(destinationMember, Expression.Default(destinationPropertyType)),
                assignNonNull);
        }

        return Expression.Block(
            new[] { value },
            Expression.Assign(value, Expression.Property(source, sourceProperty)),
            assignment);
    }

    private static Expression BuildSimpleAssignment(
        ParameterExpression destination,
        PropertyInfo destinationProperty,
        Expression value)
    {
        var destinationPropertyType = destinationProperty.PropertyType;
        var destinationMember = Expression.Property(destination, destinationProperty);

        if (destinationPropertyType.IsAssignableFrom(value.Type))
        {
            return Expression.Assign(destinationMember, Expression.Convert(value, destinationPropertyType));
        }

        if (Nullable.GetUnderlyingType(value.Type) == destinationPropertyType)
        {
            return Expression.Assign(destinationMember, Expression.Property(value, nameof(Nullable<int>.Value)));
        }

        if (Nullable.GetUnderlyingType(destinationPropertyType) == value.Type)
        {
            return Expression.Assign(destinationMember, Expression.Convert(value, destinationPropertyType));
        }

        return BuildReflectionAssignment(destination, destinationProperty, value);
    }

    private static Expression BuildReflectionAssignment(
        ParameterExpression destination,
        PropertyInfo destinationProperty,
        Expression value)
    {
        return Expression.Call(
            Expression.Constant(destinationProperty),
            typeof(PropertyInfo).GetMethod(nameof(PropertyInfo.SetValue), new[] { typeof(object), typeof(object) })!,
            Expression.Convert(destination, typeof(object)),
            Expression.Convert(value, typeof(object)));
    }

    private void AddConfiguredPropertyAssignments(ParameterExpression source, ParameterExpression destination, List<Expression> body)
    {
        foreach (var mapping in _memberMappings.Values)
        {
            if (mapping.Ignored || mapping.SourceGetter is null)
            {
                continue;
            }

            var getterType = mapping.SourceGetter.GetType();
            var destinationProperty = mapping.DestinationProperty;
            Expression value;

            if (getterType.IsGenericType
                && getterType.GetGenericTypeDefinition() == typeof(Func<,>)
                && getterType.GetGenericArguments()[0].IsAssignableFrom(typeof(TSource)))
            {
                value = Expression.Invoke(
                    Expression.Constant(mapping.SourceGetter, getterType),
                    Expression.Convert(source, getterType.GetGenericArguments()[0]));
            }
            else
            {
                value = Expression.Call(
                    Expression.Constant(mapping.SourceGetter, typeof(Delegate)),
                    typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke))!,
                    Expression.NewArrayInit(typeof(object), Expression.Convert(source, typeof(object))));
            }

            if (destinationProperty.PropertyType.IsAssignableFrom(value.Type))
            {
                body.Add(Expression.Assign(
                    Expression.Property(destination, destinationProperty),
                    Expression.Convert(value, destinationProperty.PropertyType)));
            }
            else
            {
                body.Add(BuildReflectionAssignment(destination, destinationProperty, value));
            }
        }
    }

    private static Func<TDestination> CreateDestinationFactory()
    {
        var destinationType = typeof(TDestination);

        if (destinationType.IsValueType)
        {
            return static () => default!;
        }

        if (!destinationType.IsAbstract && destinationType.GetConstructor(Type.EmptyTypes) is { } constructor)
        {
            return Expression.Lambda<Func<TDestination>>(Expression.New(constructor)).Compile();
        }

        return Activator.CreateInstance<TDestination>;
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