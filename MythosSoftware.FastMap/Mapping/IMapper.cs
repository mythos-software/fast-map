namespace MythosSoftware.FastMap;

/// <summary>
/// Defines a contract for mapping objects from one type to another.
/// </summary>
public interface IMapper
{
    TDestination Map<TDestination>(object source);
    
    TDestination Map<TSource, TDestination>(TSource source);
    
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}