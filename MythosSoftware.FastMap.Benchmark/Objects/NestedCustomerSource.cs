namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class NestedCustomerSource
{
    public string Name { get; set; } = string.Empty;
    
    public NestedAddressSource Address { get; set; } = new();
}