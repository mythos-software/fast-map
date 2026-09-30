namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class NestedCustomerDestination
{
    public string Name { get; set; } = string.Empty;
    
    public NestedAddressDestination Address { get; set; } = new();
}