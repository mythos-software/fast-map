namespace MythosSoftware.FastMap.MappingProcessors;

public interface IValueResolver<TSource, TDestination, TMember>
{
    TMember Resolve(TSource source, TDestination destination, TMember currentValue);
}
