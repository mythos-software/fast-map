using System.Reflection;

namespace MythosSoftware.FastMap;

internal sealed class MemberMapping
{
    public required PropertyInfo DestinationProperty { get; init; }

    public bool Ignored { get; init; }

    public Delegate? SourceGetter { get; init; }
}