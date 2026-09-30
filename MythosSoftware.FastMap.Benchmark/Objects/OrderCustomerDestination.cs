namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class OrderCustomerDestination
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public OrderAddressDestination? Address { get; set; }
}