namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class NestedDestination
{
    public int Id { get; set; }
    
    public NestedCustomerDestination Customer { get; set; } = new();
}