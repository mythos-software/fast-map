namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class OrderCustomerSource
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public OrderAddressSource? Address { get; set; }
}