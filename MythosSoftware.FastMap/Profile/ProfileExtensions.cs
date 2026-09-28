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
    
    public static void ReverseMap<
        TSource,
        TDestination>(this IMappingProcessor<TSource, TDestination> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        
        if (mapping is not DefaultMappingProcessor<TSource, TDestination> currentProcessor)
        {
            throw new InvalidOperationException($"ForMember is not supported by {mapping.GetType().Name}.");
        }
        
        var mappings = new Dictionary<string, MemberMapping>();
        var processor = new DefaultMappingProcessor<TDestination, TSource>(currentProcessor.Profile);
        
        foreach (var memberMapping in currentProcessor.MemberMappings)
        {
            var profile = currentProcessor.GetReversedMemberMapping(memberMapping.Value);

            if (profile != null)
            {
                mappings[profile.DestinationProperty.Name] = profile;
            }
        }
        
        processor.AddMemberMapping(mappings);
        
        currentProcessor.Profile?.RegisterProcessor(processor);
    }
}