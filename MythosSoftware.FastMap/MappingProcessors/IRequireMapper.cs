namespace MythosSoftware.FastMap.MappingProcessors;

/// <summary>
/// Represents a mapping processor that requires access to the DefaultMapper instance for mapping operations.
/// </summary>
internal interface IRequireMapper
{
    void SetMapper(DefaultMapper mapper);
}
