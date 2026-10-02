namespace MythosSoftware.FastMap.MappingProcessors;

public sealed class MemberOptions<TSource, TDestination, TMember>
{
    internal Func<TSource, TDestination, TMember, TMember>? Resolve { get; private set; }

    public void MapFrom<TResolver>() where TResolver : IValueResolver<TSource, TDestination, TMember>, new()
    {
        var resolver = new TResolver();

        Resolve = (source, destination, currentValue) => resolver.Resolve(source, destination, currentValue);
    }
}