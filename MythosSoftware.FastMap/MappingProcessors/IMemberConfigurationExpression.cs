using System.Linq.Expressions;

namespace MythosSoftware.FastMap.MappingProcessors;

public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    void Ignore();

    void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression);
}