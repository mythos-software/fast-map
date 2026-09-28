using System.Reflection;

namespace MythosSoftware.FastMap.MappingProcessors;

internal static class MappingProcessorInvoker
{
    public static object Map(object source, Type sourceType, Type destinationType)
    {
        var method = typeof(MappingProcessorInvoker)
            .GetMethod(
                nameof(MapGeneric),
                BindingFlags.NonPublic |
                BindingFlags.Static)!
            .MakeGenericMethod(
                sourceType,
                destinationType);

        return method.Invoke(null, new[] { source })!;
    }

    private static object MapGeneric<TSource, TDestination>(object source)
    {
        var processor = MappingProcessorBuilder<TSource, TDestination>.Build();

        return processor.Process((TSource)source)!;
    }
}