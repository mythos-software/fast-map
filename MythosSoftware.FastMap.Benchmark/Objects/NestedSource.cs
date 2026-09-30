namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class NestedSource
{
    public int Id { get; set; }
    
    public NestedCustomerSource Customer { get; set; } = new();
}