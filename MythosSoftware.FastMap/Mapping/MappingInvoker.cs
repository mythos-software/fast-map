namespace MythosSoftware.FastMap;

static class MappingInvoker<TDestination>
{
    public static TDestination Invoke<TSource>(DefaultMapper mapper, TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }
}