using System.Linq.Expressions;
using MythosSoftware.FastMap.MappingProcessors;

namespace MythosSoftware.FastMap;

public static class ProfileExtensions
{
    public static IMappingProcessor<TSource, TDestination> ForMember<
        TSource,
        TDestination,
        TMember>(
        this IMappingProcessor<TSource, TDestination> mapping,
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(destinationMember);
        ArgumentNullException.ThrowIfNull(options);

        var configuration =
            new MemberConfigurationExpression<
                TSource,
                TDestination,
                TMember>();

        options(configuration);
        
        if (mapping is not DefaultMappingProcessor<TSource, TDestination> processor)
        {
            throw new InvalidOperationException($"ForMember is not supported by {mapping.GetType().Name}.");
        }

        processor.AddMemberMapping(destinationMember, configuration);

        return mapping;
    }
}