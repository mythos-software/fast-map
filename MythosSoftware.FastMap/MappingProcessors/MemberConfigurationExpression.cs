using System.Linq.Expressions;

namespace MythosSoftware.FastMap.MappingProcessors;

internal sealed class MemberConfigurationExpression<TSource, TDestination, TMember>
    : IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    public bool IsIgnored { get; private set; }

    public LambdaExpression? SourceExpression { get; private set; }

    public void Ignore()
    {
        IsIgnored = true;
    }

    public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression)
    {
        SourceExpression = sourceExpression ?? throw new ArgumentNullException(nameof(sourceExpression));
    }
}